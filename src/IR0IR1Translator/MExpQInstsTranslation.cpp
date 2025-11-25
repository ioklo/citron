#include "MExpQInstsTranslation.h"

#include <expected>
#include <variant>
#include <vector>
#include <unordered_map>

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Variants.h"
#include "Infra/Unreachable.h"

#include "MIR/MExp.h"
#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QInsts.h"
#include "QIR/QArgs.h"

#include "MLocQInstsTranslation.h"
#include "QBodyContext.h"
#include "CommonQInstsTranslation.h"

using namespace std;

namespace Citron {

namespace IR0IR1Translator {
// MExp는 QInst와 1:1로 대응되지는 않는다

class MExpQInstsTranslator
{
public:
    using ResultType = expected<void, DiagPtr>;

private:
    optional<QArg_Loc> oDest;
    QBodyContext& bodyContext;

public:
    MExpQInstsTranslator(optional<QArg_Loc> oDest, QBodyContext& bodyContext)
        : oDest{oDest}, bodyContext{bodyContext}
    { }

    void EmitLoad(QArg_Loc dest, QArg_Loc pSrc, QType* qType)
    {
        visit(overloaded{
            [this, qType, pSrc](QArg_Register& destReg)
            {
                // 새로운 value 도입
                auto oRegType = bodyContext.GetRegisterType(qType);
                assert(oRegType);

                bodyContext.EmitInst(QInst_Load{*oRegType, destReg, pSrc});
            },

            [this, qType, srcLoc = Cast<QArg_Input>(pSrc)](QArg_StackSlot& destSlot)
            {
                auto size = bodyContext.GetQTypeSize(qType);
                bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Memcpy_Ptr_Ptr_Int, nullopt, {destSlot, srcLoc, QArg_ConstInt32{(int)size}});
            }
        }, dest);
    }

    // load(loc),
    ResultType Visit(MExp_Load* exp)
    {
        // 이 translation으로 lv가 하나 나올 것이다
        auto eSrcLoc = TranslateMLocToQInsts(exp->loc, bodyContext); // slot 또는 ptr이 담긴 register 
        RETURN_ON_ERROR(eSrcLoc);

        // dest를 할당할일이 없으면, loc까지만 실행하고 종료
        if (!oDest) return {};

        auto* qType = bodyContext.GetMExpQType(exp);
        EmitLoad(*oDest, *eSrcLoc, qType);

        return {};
    }

    // Assign(loc dest, src exp), Store
    // *loc = exp;
    ResultType Visit(MExp_Assign* exp)
    {
        auto* qType = bodyContext.GetMExpQType(exp);

        // dest를 먼저 계산한다
        auto eDest = TranslateMLocToQInsts(exp->dest, bodyContext);
        RETURN_ON_ERROR(eDest);

        // src계산
        auto eSrc = TranslateMExpToQInsts(exp->src, *eDest, bodyContext);
        RETURN_ON_ERROR(eSrc);

        // *oDest에 **eDest를 넣어야 한다
        // *oDest: T, *eDest: T*
        if (oDest)
        {   
            auto* qType = bodyContext.GetMExpQType(exp->src);
            EmitLoad(*oDest, *eDest, qType);
        }

        // src를 평가한 값을 그대로 리턴
        return {};
    }

    // box 3;
    // IR1에서 힙으로 값을 올리려면,
    // runtime 함수를 콜 하고,
    // 그 위치에 exp를 넣도록
    ResultType Visit(MExp_Box* exp)
    {
        throw NotImplementedException{};

        //// 1. alloc, size
        //auto* qType = bodyContext.GetMExpQType(exp->innerExp);
        // auto size = bodyContext.GetQTypeSize(qType);
        //auto ptr = bodyContext.AddIntrinsic(QInst_IntrinsicKind::Alloc_Int, {QArg_ConstInt32{(int)size}});

        //// 2. exp
        //auto eQValue = TranslateMExpToQInsts(exp->innerExp, bodyContext);
        //RETURN_ON_ERROR(eQValue);

        //// 3. 저장
        //bodyContext.AddInst(QInst_Store{.dest = ptr, .src = *eQValue});
        //return *eQValue;
    }
    
    // box*의 구조, {sentinel, offset}
    ResultType Visit(MExp_StaticBoxRef* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_ClassMemberBoxRef* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_StructIndirectMemberBoxRef* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_StructMemberBoxRef* exp) { throw NotImplementedException{}; }

    // LocalRef (Loc)
    ResultType Visit(MExp_LocalRef* exp)
    {
        //// local ref는 &s.a 같은 걸 수 있다
        //auto eLV = TranslateMLocToQInsts(exp->innerLoc, bodyContext);
        //RETURN_ON_ERROR(eLV);

        //// lv 그대로 리턴하면 될거 같다
        //return *eLV;
        throw NotImplementedException{};
    }

    ResultType Visit(MExp_BoolLiteral* exp)
    {
        if (!oDest) return {}; // nested가 없으니 바로 리턴한다

        visit(overloaded{
            [this, exp](QArg_Register& destReg)
            {
                bodyContext.EmitInst(QInst_Assign{QRegisterType::Int1, destReg, QArg_ConstBool{exp->value}});
            },

            [this, exp](QArg_StackSlot& destSlot)
            {
                bodyContext.EmitInst(QInst_Store{QRegisterType::Int1, destSlot, QArg_ConstBool{exp->value}});
            }
        }, *oDest);

        return {};
    }

    ResultType Visit(MExp_IntLiteral* exp)
    {
        if (!oDest) return {};

        visit(overloaded{
            [this, exp](QArg_Register& destReg)
            {
                bodyContext.EmitInst(QInst_Assign{QRegisterType::Int32, destReg, QArg_ConstInt32{exp->value}});
            },

            [this, exp](QArg_StackSlot& destSlot) // slot은 stack을 가리키는 포인터
            {
                bodyContext.EmitInst(QInst_Store{QRegisterType::Int32, destSlot, QArg_ConstInt32{exp->value}});
            }
        }, *oDest);
        return {};
    }

    ResultType Visit(MExp_String* exp)
    {
        if (oDest)
            return TranslateMExp_StringToQInsts(exp, get<QArg_StackSlot>(*oDest), bodyContext);
            
        return TranslateMExp_StringToQInsts(exp, nullopt, bodyContext);
    }

    ResultType Visit(MExp_List* exp) 
    {
        /*std::vector<QArg_Value> items;
        items.reserve(exp->elems.size());

        for (auto* elem : exp->elems)
        {
            auto eItem = TranslateMExpToQInsts(elem, bodyContext);
            RETURN_ON_ERROR(eItem);

            items.push_back(*eItem);
        }

        auto result = bodyContext.AddIntrinsic(QInst_IntrinsicKind::NewList_Items, std::move(items));
        return *result;*/
        throw NotImplementedException{};
    }

    ResultType Visit(MExp_ListIterator* exp)
    {
        /*auto eLV = TranslateMLocToQInsts(exp->listLoc, bodyContext);
        RETURN_ON_ERROR(eLV);

        auto result = bodyContext.AddIntrinsic(QInst_IntrinsicKind::GetListIterator_List, {*eLV});
        return *result;*/
        throw NotImplementedException{};
    }

    ResultType Visit(MExp_CallInternalUnaryOperator* exp) 
    { 
        auto operand = bodyContext.NewContainerForMExp(exp->operand);
        auto eOperand = TranslateMExpToQInsts(exp->operand, operand, bodyContext);
        RETURN_ON_ERROR(eOperand);

        static unordered_map<MInternalUnaryOperator, QInst_IntrinsicKind> m{
            {MInternalUnaryOperator::LogicalNot_Bool_Bool, QInst_IntrinsicKind::LogicalNot_Bool},
            {MInternalUnaryOperator::UnaryMinus_Int_Int, QInst_IntrinsicKind::UnaryMinus_Int},
            {MInternalUnaryOperator::ToString_Bool_String, QInst_IntrinsicKind::ToString_Bool},
            {MInternalUnaryOperator::ToString_Int_String, QInst_IntrinsicKind::ToString_Int}
        };

        auto i = m.find(exp->op);
        assert(i != m.end());

        if (oDest)
            bodyContext.EmitIntrinsic(i->second, *oDest, {Cast<QArg_Input>(operand)});

        return {};
    }

    ResultType Visit(MExp_CallInternalUnaryAssignOperator* exp)
    {
        // exp->operand는 항상 lvalue이다
        auto eOperand = TranslateMLocToQInsts(exp->operand, bodyContext);
        RETURN_ON_ERROR(eOperand);

        static unordered_map<MInternalUnaryAssignOperator, QInst_IntrinsicKind> m{
            {MInternalUnaryAssignOperator::PrefixInc_Int_Int, QInst_IntrinsicKind::PrefixInc_Int},
            {MInternalUnaryAssignOperator::PrefixDec_Int_Int, QInst_IntrinsicKind::PrefixDec_Int},
            {MInternalUnaryAssignOperator::PostfixInc_Int_Int, QInst_IntrinsicKind::PostfixInc_Int},
            {MInternalUnaryAssignOperator::PostfixDec_Int_Int, QInst_IntrinsicKind::PostfixDec_Int},
        };

        auto i = m.find(exp->op);
        assert(i != m.end());

        if (oDest)
            bodyContext.EmitIntrinsic(i->second, *oDest, {Cast<QArg_Input>(*eOperand)});

        return {};
    }

    ResultType Visit(MExp_CallInternalBinaryOperator* exp)
    {
        auto operand0 = bodyContext.NewContainerForMExp(exp->operand0);
        auto eOperand0 = TranslateMExpToQInsts(exp->operand0, operand0, bodyContext);
        RETURN_ON_ERROR(eOperand0);

        auto operand1 = bodyContext.NewContainerForMExp(exp->operand1);
        auto eOperand1 = TranslateMExpToQInsts(exp->operand1, operand1, bodyContext);
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

        if (oDest)
            bodyContext.EmitIntrinsic(i->second, *oDest, {Cast<QArg_Input>(operand0), Cast<QArg_Input>(operand1)});
        return {};
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
                    throw NotImplementedException{};
                    // TranslateMExpToQInsts(normalArg.exp, bodyContext);
                },

                // 파라미터를 여러개 받는 경우
                [](MArgument_Params& paramsArg)
                {
                    throw NotImplementedException{};
                }
            }, arg);
        }
        throw NotImplementedException{};
        // bodyContext.AddInst(QInst_Call{exp->funcDecl, }
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

expected<void, DiagPtr> TranslateMExpToQInsts(MExp* mExp, std::optional<QArg_Loc> oDest, QBodyContext& bodyContext)
{
    MExpQInstsTranslator translator{oDest, bodyContext};
    return Accept(translator, mExp);
}

} // namesapce IR0IR1Tranaslator

} // namespace Citron
