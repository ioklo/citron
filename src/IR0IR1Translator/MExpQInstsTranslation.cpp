#include "MExpQInstsTranslation.h"

#include <expected>
#include <variant>
#include <unordered_map>

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Variants.h"
#include "Infra/Unreachable.h"

#include "MIR/MExp.h"
#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QInsts.h"
#include "QIR/QValues.h"

#include "MLocQInstsTranslation.h"
#include "QBodyContext.h"

using namespace std;

namespace Citron {

namespace IR0IR1Translator {
// MExp는 QInst와 1:1로 대응되지는 않는다

class MExpQInstsTranslator
{
public:
    using ResultType = expected<QValue, DiagPtr>;
    QBodyContext& qBodyContext;

public:
    MExpQInstsTranslator(QBodyContext& qBodyContext)
        : qBodyContext{qBodyContext}{ }

    // load(loc),
    ResultType Visit(MExp_Load* exp)
    {
        // 이 translation으로 lv가 하나 나올 것이다
        auto eLV = TranslateMLocToQInsts(exp->loc, qBodyContext);
        RETURN_ON_ERROR(eLV);

        // 새로운 value 도입
        auto lv = qBodyContext.NewValue();
        qBodyContext.AddInst(QInst_Load{lv, *eLV});
        return lv;
    }

    // Assign(loc dest, src exp)
    ResultType Visit(MExp_Assign* exp)
    {
        // dest를 먼저 계산한다
        auto eLV = TranslateMLocToQInsts(exp->dest, qBodyContext);
        RETURN_ON_ERROR(eLV);

        // src계산
        auto eV = TranslateMExpToQInsts(exp->src, qBodyContext);
        RETURN_ON_ERROR(eV);

        qBodyContext.AddInst(QInst_Store{*eLV, *eV});

        // src를 평가한 값을 그대로 리턴
        return eV;
    }

    // box 3;
    // IR1에서 힙으로 값을 올리려면,
    // runtime 함수를 콜 하고,
    // 그 위치에 exp를 넣도록
    ResultType Visit(MExp_Box* exp)
    {
        // 1. alloc, size
        auto lv = qBodyContext.NewValue();
        size_t size = qBodyContext.GetExpTypeSize(exp->innerExp);
        qBodyContext.AddInst(QInst_Alloc{lv, size});

        // 2. exp
        auto eQValue = TranslateMExpToQInsts(exp->innerExp, qBodyContext);
        RETURN_ON_ERROR(eQValue);

        // 3. 저장
        qBodyContext.AddInst(QInst_Store{lv, *eQValue});
        return *eQValue;
    }
    
    // box*의 구조, {sentinel, offset}
    ResultType Visit(MExp_StaticBoxRef* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_ClassMemberBoxRef* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_StructIndirectMemberBoxRef* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_StructMemberBoxRef* exp) { throw NotImplementedException{}; }

    // LocalRef (Loc)
    ResultType Visit(MExp_LocalRef* exp)
    {
        // local ref는 &s.a 같은 걸 수 있다
        auto eLV = TranslateMLocToQInsts(exp->innerLoc, qBodyContext);
        RETURN_ON_ERROR(eLV);

        // lv 그대로 리턴하면 될거 같다
        return *eLV;
    }

    ResultType Visit(MExp_BoolLiteral* exp)
    {
        return QValue_ConstBool{exp->value};
    }

    ResultType Visit(MExp_IntLiteral* exp)
    {
        return QValue_ConstInteger{exp->value};
    }

    ResultType Visit(MExp_String* exp)
    {
        // "abc $x" => "abc " + x
        optional<QValue> curValue;
        for(auto& elem : exp->elements)
        {
            auto eValueResult = visit(overloaded{
                [this](MExp_StringElem_Text& textElem) { return expected<QValue, DiagPtr>{QValue_String{textElem.text}}; },
                [this](MExp_StringElem_Exp& expElem) { return TranslateMExpToQInsts(expElem.mExp, qBodyContext); }
            }, elem);
            RETURN_ON_ERROR(eValueResult);

            if (curValue)
            {   
                QValue_Named newValue;
                qBodyContext.AddIntrinsic(QInst_IntrinsicKind::Add_String_String, {newValue, *curValue, *eValueResult});
                curValue = newValue;
            }
            else
            {
                curValue = *eValueResult;
            }
        }

        assert(curValue);
        return *curValue;
    }

    ResultType Visit(MExp_List* exp) 
    {
        std::vector<QValue> items;
        items.reserve(exp->elems.size());

        for (auto* elem : exp->elems)
        {
            auto eItem = TranslateMExpToQInsts(elem, qBodyContext);
            RETURN_ON_ERROR(eItem);

            items.push_back(*eItem);
        }

        auto result = qBodyContext.AddIntrinsic(QInst_IntrinsicKind::NewList_Items, std::move(items));
        return result;
    }

    ResultType Visit(MExp_ListIterator* exp)
    {
        auto eLV = TranslateMLocToQInsts(exp->listLoc, qBodyContext);
        RETURN_ON_ERROR(eLV);

        auto result = qBodyContext.AddIntrinsic(QInst_IntrinsicKind::GetListIterator_List, {*eLV});
        return result;
    }

    ResultType Visit(MExp_CallInternalUnaryOperator* exp) 
    { 
        auto eOperand = TranslateMExpToQInsts(exp, qBodyContext);
        RETURN_ON_ERROR(eOperand);

        static unordered_map<MInternalUnaryOperator, QInst_IntrinsicKind> m{
            {MInternalUnaryOperator::LogicalNot_Bool_Bool, QInst_IntrinsicKind::LogicalNot_Bool},
            {MInternalUnaryOperator::UnaryMinus_Int_Int, QInst_IntrinsicKind::UnaryMinus_Int},
            {MInternalUnaryOperator::ToString_Bool_String, QInst_IntrinsicKind::ToString_Bool},
            {MInternalUnaryOperator::ToString_Int_String, QInst_IntrinsicKind::ToString_Int}
        };

        auto i = m.find(exp->op);
        assert(i != m.end());

        return qBodyContext.AddIntrinsic(i->second, {*eOperand});
    }

    ResultType Visit(MExp_CallInternalUnaryAssignOperator* exp)
    {
        auto eOperand = TranslateMExpToQInsts(exp, qBodyContext);
        RETURN_ON_ERROR(eOperand);

        static unordered_map<MInternalUnaryAssignOperator, QInst_IntrinsicKind> m{
            {MInternalUnaryAssignOperator::PrefixInc_Int_Int, QInst_IntrinsicKind::PrefixInc_Int},
            {MInternalUnaryAssignOperator::PrefixDec_Int_Int, QInst_IntrinsicKind::PrefixDec_Int},
            {MInternalUnaryAssignOperator::PostfixInc_Int_Int, QInst_IntrinsicKind::PostfixInc_Int},
            {MInternalUnaryAssignOperator::PostfixDec_Int_Int, QInst_IntrinsicKind::PostfixDec_Int},
        };

        auto i = m.find(exp->op);
        assert(i != m.end());

        return qBodyContext.AddIntrinsic(i->second, {*eOperand});
    }

    ResultType Visit(MExp_CallInternalBinaryOperator* exp)
    {
        auto eOperand0 = TranslateMExpToQInsts(exp->operand0, qBodyContext);
        RETURN_ON_ERROR(eOperand0);

        auto eOperand1 = TranslateMExpToQInsts(exp->operand1, qBodyContext);
        RETURN_ON_ERROR(eOperand1);

        static unordered_map<MInternalBinaryOperator, QInst_IntrinsicKind> m{
            {MInternalBinaryOperator::Multiply_Int_Int_Int, QInst_IntrinsicKind::Multiply_Int_Int},
            {MInternalBinaryOperator::Divide_Int_Int_Int, QInst_IntrinsicKind::Divide_Int_Int},
            {MInternalBinaryOperator::Modulo_Int_Int_Int, QInst_IntrinsicKind::Modulo_Int_Int},
            {MInternalBinaryOperator::Add_Int_Int_Int, QInst_IntrinsicKind::Add_Int_Int},
            {MInternalBinaryOperator::Add_String_String_String, QInst_IntrinsicKind::Add_String_String},
            {MInternalBinaryOperator::Subtract_Int_Int_Int, QInst_IntrinsicKind::Subtract_Int_Int},
            {MInternalBinaryOperator::LessThan_Int_Int_Bool, QInst_IntrinsicKind::LessThan_Int_Int},
            {MInternalBinaryOperator::LessThan_String_String_Bool, QInst_IntrinsicKind::LessThan_String_String},
            {MInternalBinaryOperator::GreaterThan_Int_Int_Bool, QInst_IntrinsicKind::GreaterThan_Int_Int},
            {MInternalBinaryOperator::GreaterThan_String_String_Bool, QInst_IntrinsicKind::GreaterThan_String_String},
            {MInternalBinaryOperator::LessThanOrEqual_Int_Int_Bool, QInst_IntrinsicKind::LessThanOrEqual_Int_Int},
            {MInternalBinaryOperator::LessThanOrEqual_String_String_Bool, QInst_IntrinsicKind::LessThanOrEqual_String_String},
            {MInternalBinaryOperator::GreaterThanOrEqual_Int_Int_Bool, QInst_IntrinsicKind::GreaterThanOrEqual_Int_Int},
            {MInternalBinaryOperator::GreaterThanOrEqual_String_String_Bool, QInst_IntrinsicKind::GreaterThanOrEqual_String_String},
            {MInternalBinaryOperator::Equal_Int_Int_Bool, QInst_IntrinsicKind::Equal_Int_Int},
            {MInternalBinaryOperator::Equal_Bool_Bool_Bool, QInst_IntrinsicKind::Equal_Bool_Bool},
            {MInternalBinaryOperator::Equal_String_String_Bool, QInst_IntrinsicKind::Equal_String_String},
        };

        auto i = m.find(exp->op);
        assert(i != m.end());

        return qBodyContext.AddIntrinsic(i->second, {*eOperand0, *eOperand1});
    }

    // GlobalFunc
    ResultType Visit(MExp_CallGlobalFunc* exp) 
    {
        // generics는 어떻게 하나요
        // T F<T>(T t) { return t; }
        // Generics는 T에 관한 정보를 더 넘겨준다 (크기 등)
        // 따라서 이 함수는 t, {F함수에 대한 constraint table} 두 인자를 받는다
        // 그리고 t는 항상 stack pointer를 가리키게 된다 (callee쪽에서 크기를 정확히 알 수 없으므로)
        for(auto& arg : exp->args)
        {
            visit(overloaded{
                [this](MArgument_Normal& normalArg) 
                {
                    TranslateMExpToQInsts(normalArg.exp, qBodyContext);
                },

                // 파라미터를 여러개 받는 경우
                [](MArgument_Params& paramsArg)
                {
                    throw NotImplementedException{};
                }
            }, arg);
        }
        throw NotImplementedException{};
        // qBodyContext.AddInst(QInst_Call{exp->funcDecl, }
    }

    ResultType Visit(MExp_NewClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CallClassFunc* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CastClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_NewStruct* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CallStructFunc* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_NewEnumElem* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CastEnumElemToEnum* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_NewNullable* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_NullableValueNullLiteral* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_NullableRefNullLiteral* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_Lambda* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CallLambda* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CastBoxedLambdaToFunc* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_InlineBlock* exp) { throw NotImplementedException{}; }

    ResultType Visit(MExp_ClassIsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_ClassAsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_ClassIsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_ClassAsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_InterfaceIsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_InterfaceAsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_InterfaceIsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_InterfaceAsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_EnumIsEnumElem* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_EnumAsEnumElem* exp) { throw NotImplementedException{}; }
};

expected<QValue, DiagPtr> TranslateMExpToQInsts(MExp* mExp, QBodyContext& bodyContext)
{
    MExpQInstsTranslator translator{bodyContext};
    return Accept(translator, mExp);
}

} // namesapce IR0IR1Tranaslator

} // namespace Citron
