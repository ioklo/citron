#include "MExpQInstsTranslation.h"

#include <expected>
#include <variant>
#include <vector>
#include <unordered_map>

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Variants.h"
#include "Infra/Unreachable.h"

#include "Logging/Diag.h"

#include "RSymbol/RGlobalFuncDecl.h"

#include "MIR/MExp.h"
#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QInsts.h"
#include "QIR/QArgs.h"

#include "MLocQInstsTranslation.h"
#include "QBodyContext.h"
#include "ScopeGuard.h"
#include "CommonQInstsTranslation.h"

using namespace std;

namespace Citron {

// MExp는 QInst와 1:1로 대응되지는 않는다
class MExpQInstsTranslator
{
public:
    using ResultType = expected<void, DiagPtr>;

private:
    optional<QArg_Slot> oDest;
    QBodyContext& bodyContext;

public:
    MExpQInstsTranslator(optional<QArg_Slot> oDest, QBodyContext& bodyContext)
        : oDest{oDest}, bodyContext{bodyContext}
    {
    }

    // load(loc),
    ResultType Visit(MExp_Load* exp)
    {
        // 이 translation으로 lv가 하나 나올 것이다
        auto eSrcLoc = TranslateMLocToQInsts(exp->loc, bodyContext); // ptr이 담긴 slot
        RETURN_ON_ERROR(eSrcLoc);

        // dest를 할당할일이 없으면, loc까지만 실행하고 종료
        if (!oDest) return {};

        return visit([this, exp](auto& srcLoc) -> expected<void, DiagPtr>
        {
            using T = remove_cvref_t<decltype(srcLoc)>;
            if constexpr (same_as<T, QLocResult_Slot>)
            {
                auto* qType = bodyContext.GetMExpQType(exp);

                // TODO: String을 일반적인 struct로
                if (qType == bodyContext.GetStringQType())
                {
                    // string이면 string의 Copy Assign을 불러줘야 한다.
                    return bodyContext.EmitInst(QInst_CopyAssign_String{*oDest, QArg_Slot{srcLoc.slotIndex}});
                }

                // primitive는 assign을 해도 된다 => CopyAssign으로 대체해야할지도
                return bodyContext.EmitInst(QInst_Assign{qType, *oDest, QArg_Slot{srcLoc.slotIndex}});
            }
            else static_assert(false);

        }, *eSrcLoc);

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
        
        return visit([this, exp](auto& destLoc) -> expected<void, DiagPtr>
        {
            using T = remove_cvref_t<decltype(destLoc)>;
            if constexpr (same_as <T, QLocResult_Slot>)
            {
                // slot이면 바로 exp계산에 참여시킬수 있다
                auto eSrc = TranslateMExpToQInsts(exp->src, QArg_Slot{destLoc.slotIndex}, bodyContext);
                RETURN_ON_ERROR(eSrc);

                // *oDest에 *eDest를 넣어야 한다
                // *oDest: T, *eDest: T
                if (oDest)
                {
                    auto* qType = bodyContext.GetMExpQType(exp->src);
                    auto eEmitResult = bodyContext.EmitInst(QInst_Assign{qType, *oDest, QArg_Slot{destLoc.slotIndex}});
                    RETURN_ON_ERROR(eEmitResult);
                }

                return {};
            }
            else static_assert(false); // 나머지는 추후에

        }, *eDest);
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

        return bodyContext.EmitInst(QInst_Assign{bodyContext.GetBoolQType(), *oDest, QArg_ConstBool{exp->value}});
    }

    ResultType Visit(MExp_IntLiteral* exp)
    {
        if (!oDest) return {};

        return bodyContext.EmitInst(QInst_Assign{bodyContext.GetIntQType(), *oDest, QArg_ConstInt32{exp->value}});
    }

    ResultType Visit(MExp_String* exp)
    {
        if (oDest)
            return TranslateMExp_StringToQInsts(exp, *oDest, bodyContext);

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
        auto operand = bodyContext.NewSlotForMExp(exp->operand);
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
            return bodyContext.EmitIntrinsic(i->second, *oDest, {operand});

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

        return visit([this, op = i->second](auto& operand) -> expected<void, DiagPtr> {

            using T = remove_cvref_t<decltype(operand)>;
            if constexpr (same_as<T, QLocResult_Slot>)
            {
                // slot to ptr
                auto* qPtrType = bodyContext.GetPtrQType();
                auto ptrSlot = bodyContext.NewSlot(qPtrType);
                auto eEmitAddrResult = bodyContext.EmitInst(QInst_AddrOf{ptrSlot, QArg_Slot{operand.slotIndex}});
                RETURN_ON_ERROR(eEmitAddrResult);

                auto dest = oDest ? *oDest : bodyContext.NewSlot(bodyContext.GetIntQType());
                auto eEmitIntrinsicResult = bodyContext.EmitIntrinsic(op, dest, {ptrSlot});
                RETURN_ON_ERROR(eEmitIntrinsicResult);
                return {};
            }
            else static_assert(false);

        }, *eOperand);
    }

    ResultType Visit(MExp_CallInternalBinaryOperator* exp)
    {
        auto operand0 = bodyContext.NewSlotForMExp(exp->operand0);
        auto eOperand0 = TranslateMExpToQInsts(exp->operand0, operand0, bodyContext);
        RETURN_ON_ERROR(eOperand0);

        auto operand1 = bodyContext.NewSlotForMExp(exp->operand1);
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
            return bodyContext.EmitIntrinsic(i->second, *oDest, {operand0, operand1});

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

        vector<QArg_Input> args;

        // 1. 결과값 선 처리, 결과값이 stack을 쓰는 경우라면 args에 첫 인자로 넣어준다
        struct RetPolicy_Void {};
        struct RetPolicy_UsingStackSlot { QType* qSlotType; };
        using RetPolicy = variant<RetPolicy_Void, RetPolicy_UsingStackSlot>;

        auto retPolicy = [this, exp, &args] -> RetPolicy {
            auto* qRetType = bodyContext.GetReturnQType(exp->rFuncDecl, *exp->rTypeArgs);

            if (bodyContext.IsVoidQType(qRetType))
            {
                assert(!oDest);
                return RetPolicy_Void{};
            }

            return RetPolicy_UsingStackSlot{qRetType};
        }();

        // 2. 인자를 args에 넣는다
        for (auto& arg : exp->args)
        {
            auto eResult = visit([this, &args](auto& arg) -> expected<void, DiagPtr>
            {
                using T = remove_cvref_t<decltype(arg)>;

                if constexpr (same_as<T, MArgument_Normal>)
                {
                    auto argDest = bodyContext.NewSlotForMExp(arg.exp);
                    auto eResult = TranslateMExpToQInsts(arg.exp, argDest, bodyContext);
                    RETURN_ON_ERROR(eResult);
                    args.push_back(argDest);
                    return {};
                }
                else if constexpr (same_as<T, MArgument_Params>) // 파라미터를 여러개 받는 경우
                {
                    throw NotImplementedException{};
                    return {};
                }
                else static_assert(false);
            }, arg);
            RETURN_ON_ERROR(eResult);
        }

        // 3. Emit처리
        return visit([this, exp, &args](auto& retPolicy) -> expected<void, DiagPtr>
        {
            using T = remove_cvref_t<decltype(retPolicy)>;
            if constexpr (same_as<T, RetPolicy_Void>)
            {
                return bodyContext.EmitInst(QInst_Call{exp->rFuncDecl, nullopt, move(args)});
            }
            else if constexpr (same_as<T, RetPolicy_UsingStackSlot>)
            {
                QArg_Slot resultSlot = oDest ? *oDest : bodyContext.NewSlot(retPolicy.qSlotType);
                return bodyContext.EmitInst(QInst_Call{exp->rFuncDecl, resultSlot, move(args)});
            }
            else static_assert(false);

        }, retPolicy);
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

expected<void, DiagPtr> TranslateMExpToQInsts(MExp* mExp, optional<QArg_Slot> oDest, QBodyContext& bodyContext)
{
    MExpQInstsTranslator translator{oDest, bodyContext};
    return Accept(translator, mExp);
}

expected<void, DiagPtr> TranslateMExpToQInstsWithNewScope(MExp* mExp, optional<QArg_Slot> oDest, QBodyContext& bodyContext)
{
    ScopeGuard expGuard{bodyContext};
    return TranslateMExpToQInsts(mExp, oDest, bodyContext);
}

} // namespace Citron
