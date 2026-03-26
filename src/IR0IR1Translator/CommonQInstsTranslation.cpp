#include "CommonQInstsTranslation.h"
#include <optional>
#include <ranges>
#include <cassert>

#include "Infra/Variants.h"
#include "Infra/Expected.h"
#include "RSymbol/RFactory.h"
#include "MIR/MExp.h"
#include "MIR/MInitExp.h"
#include "QTranslationContexts.h"
#include "QBodyContext.h"
#include "ScopeGuard.h"
#include "MLocToQInsts.h"
#include "MCreateToQInsts.h"
#include "MReadToQInsts.h"

using namespace std;

namespace Citron {

namespace {

expected<void, DiagPtr> TranslateMInitExp_StringElemToQInsts(MInitExp_StringElem& elem, optional<size_t> o_destSlotIndex, QTranslationContexts& contexts)
{
    return visit([&contexts, &o_destSlotIndex](auto& elem) -> expected<void, DiagPtr> {
        using T = remove_cvref_t<decltype(elem)>;
        if constexpr (same_as<T, MInitExp_StringElem_Text>)
        {
            if (o_destSlotIndex)
                return contexts.bodyContext.EmitInst(QInst_Ctor_String{QArg_Slot{*o_destSlotIndex}, elem.text});

            return {};
        }
        else if constexpr (same_as<T, MInitExp_StringElem_Exp>)
        {
            return TranslateMRead_LocToQInsts(elem.loc, o_destSlotIndex, contexts);
        }
        else static_assert(false);
    }, elem);
}
} // namespace 

expected<QArg_Input, DiagPtr> TranslateMArgumentToQInsts(MArgument& arg, QTranslationContexts& contexts)
{
    return visit([&contexts](auto& arg) -> expected<QArg_Input, DiagPtr> {
        using T = remove_cvref_t<decltype(arg)>;

        if constexpr (same_as<T, MArgument_Create>)
        {
            // argument를 위한 slot을 하나 마련한다
            auto* argType = GetType(arg.create, &*contexts.rFactory);
            auto argSlotIndex = contexts.bodyContext.NewSlot(argType);

            auto e_result = TranslateMCreateToQInsts(arg.create, argSlotIndex, contexts);
            RETURN_ON_ERROR(e_result);

            return QArg_Slot{argSlotIndex};
        }
        else if constexpr (same_as <T, MArgument_Loc>) // loc은 pointer로 넘긴다
        {
            auto e_locResult = TranslateMLocToQInsts(arg.loc, contexts);
            RETURN_ON_ERROR(e_locResult);

            return visit([&contexts](auto& locResult) -> expected<QArg_Input, DiagPtr> {
                using U = remove_cvref_t<decltype(locResult)>;

                if constexpr (same_as<U, QLocResult_Slot>)
                {
                    // slot이면 addrOf를 써서 ptr로 만든다
                    auto* rPtrType = contexts.bodyContext.GetPtrType();
                    auto ptrSlotIndex = contexts.bodyContext.NewSlot(rPtrType);
                    auto e_emitAddrResult = contexts.bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{ptrSlotIndex}, QArg_Slot{locResult.slotIndex}});
                    RETURN_ON_ERROR(e_emitAddrResult);

                    return QArg_Slot{ptrSlotIndex};
                }
                else if constexpr (same_as<U, QLocResult_Ptr>)
                {
                    // ptr이면 그대로 넣어준다
                    return QArg_Slot{locResult.slotIndex};
                }
                else static_assert(false);
            }, *e_locResult);
        }
        else if constexpr (same_as<T, MArgument_Move>)
        {
            // TODO: [30] move구현
            throw NotImplementedException{};
        }
        else if constexpr (same_as<T, MArgument_Forward>)
        {
            // TODO: [57] [forward] ref parameter 지원
            throw NotImplementedException{};
        }
        else if constexpr (same_as<T, MArgument_Params>) // 파라미터를 여러개 받는 경우
        {
            // TODO: [31] params 구현
            throw NotImplementedException{};
        }
        else static_assert(false);
    }, arg);
}

expected<vector<QArg_Input>, DiagPtr> TranslateMArgumentsToQInsts(vector<MArgument>& mArgs, QTranslationContexts& contexts)
{
    vector<QArg_Input> qArgs;
    qArgs.reserve(mArgs.size());

    for (auto& mArg : mArgs)
    {
        auto e_qArg = TranslateMArgumentToQInsts(mArg, contexts);
        RETURN_ON_ERROR(e_qArg);

        qArgs.push_back(move(*e_qArg));
    }

    return move(qArgs);
}

QIntrinsicInfo* GetIntrinsicInfo(MExp_CallIntrinsicKind kind, QTranslationContexts& contexts)
{
    static unordered_map<MExp_CallIntrinsicKind, QIntrinsicInfo> m = [&contexts]() {
        auto* boolType = contexts.rFactory->MakeBoolType();
        auto* intType = contexts.rFactory->MakeIntType();

        return unordered_map<MExp_CallIntrinsicKind, QIntrinsicInfo>{
            {MExp_CallIntrinsicKind::LogicalNot_Bool_Bool, {.kind = QInst_IntrinsicKind::LogicalNot_Bool_Bool, .type = boolType} },
            {MExp_CallIntrinsicKind::UnaryMinus_Int_Int, {.kind = QInst_IntrinsicKind::UnaryMinus_Int_Int, .type = intType}},
            {MExp_CallIntrinsicKind::PrefixInc_Int_Int, {.kind = QInst_IntrinsicKind::PrefixInc_Int_Int, .type = intType}},
            {MExp_CallIntrinsicKind::PrefixDec_Int_Int, {.kind = QInst_IntrinsicKind::PrefixDec_Int_Int, .type = intType}},
            {MExp_CallIntrinsicKind::PostfixInc_Int_Int, {.kind = QInst_IntrinsicKind::PostfixInc_Int_Int, .type = intType}},
            {MExp_CallIntrinsicKind::PostfixDec_Int_Int, {.kind = QInst_IntrinsicKind::PostfixDec_Int_Int, .type = intType}},
            {MExp_CallIntrinsicKind::Multiply_Int_Int_Int, {.kind = QInst_IntrinsicKind::Multiply_Int_Int_Int, .type = intType}},
            {MExp_CallIntrinsicKind::Divide_Int_Int_Int, {.kind = QInst_IntrinsicKind::Divide_Int_Int_Int, .type = intType}},
            {MExp_CallIntrinsicKind::Modulo_Int_Int_Int, {.kind = QInst_IntrinsicKind::Modulo_Int_Int_Int, .type = intType}},
            {MExp_CallIntrinsicKind::Add_Int_Int_Int, {.kind = QInst_IntrinsicKind::Add_Int_Int_Int, .type = intType}},
            {MExp_CallIntrinsicKind::Subtract_Int_Int_Int, {.kind = QInst_IntrinsicKind::Subtract_Int_Int_Int, .type = intType}},
            {MExp_CallIntrinsicKind::LessThan_Int_Int_Bool, {.kind = QInst_IntrinsicKind::LessThan_Int_Int_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::LessThan_String_String_Bool, {.kind = QInst_IntrinsicKind::LessThan_String_String_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::GreaterThan_Int_Int_Bool, {.kind = QInst_IntrinsicKind::GreaterThan_Int_Int_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::GreaterThan_String_String_Bool, {.kind = QInst_IntrinsicKind::GreaterThan_String_String_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::LessThanOrEqual_Int_Int_Bool, {.kind = QInst_IntrinsicKind::LessThanOrEqual_Int_Int_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::LessThanOrEqual_String_String_Bool, {.kind = QInst_IntrinsicKind::LessThanOrEqual_String_String_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::GreaterThanOrEqual_Int_Int_Bool, {.kind = QInst_IntrinsicKind::GreaterThanOrEqual_Int_Int_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::GreaterThanOrEqual_String_String_Bool, {.kind = QInst_IntrinsicKind::GreaterThanOrEqual_String_String_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::Equal_Int_Int_Bool, {.kind = QInst_IntrinsicKind::Equal_Int_Int_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::Equal_Bool_Bool_Bool, {.kind = QInst_IntrinsicKind::Equal_Bool_Bool_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::Equal_String_String_Bool, {.kind = QInst_IntrinsicKind::Equal_String_String_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::GetIterator_List_ListIterator, {.kind = QInst_IntrinsicKind::GetIterator_List_ListIterator, .type = nullptr}} // TODO:
        };
        }();

    auto i = m.find(kind);
    if (i == m.end()) return nullptr;

    return &i->second;
}


expected<void, DiagPtr> TranslateMInitExp_StringToQInsts(MInitExp_String* exp, optional<size_t> destSlotIndex, QTranslationContexts& contexts)
{
    assert(!exp->elements.empty());

    // 원소가 한개라면, dest에 직접 넣는다
    if (exp->elements.size() == 1)
    {
        auto e_result = TranslateMInitExp_StringElemToQInsts(exp->elements.front(), destSlotIndex, contexts);
        RETURN_ON_ERROR(e_result);
    }
    else
    {
        auto* stringType = contexts.bodyContext.GetStringType();

        // "abc $x" => "abc " + x
        auto curSlotIndex = contexts.bodyContext.NewSlot(stringType);
        auto e_resultFront = TranslateMInitExp_StringElemToQInsts(exp->elements.front(), curSlotIndex, contexts);
        RETURN_ON_ERROR(e_resultFront);

        auto elemSlotIndex = contexts.bodyContext.NewSlot(stringType);
        auto newSlotIndex = contexts.bodyContext.NewSlot(stringType);
        for (size_t i = 1, end = exp->elements.size() - 1; i < end; i++)
        {   
            auto e_result = TranslateMInitExp_StringElemToQInsts(exp->elements[i], elemSlotIndex, contexts);
            RETURN_ON_ERROR(e_result);
            
            auto e_emitResult = contexts.bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Add_String_String, QArg_Slot{newSlotIndex}, {QArg_Slot{curSlotIndex}, QArg_Slot{elemSlotIndex}});
            RETURN_ON_ERROR(e_emitResult);

            swap(curSlotIndex, newSlotIndex);
        }
        
        auto e_resultBack = TranslateMInitExp_StringElemToQInsts(exp->elements.back(), elemSlotIndex, contexts);
        RETURN_ON_ERROR(e_resultBack);

        if (destSlotIndex)
        {
            auto e_emitResult = contexts.bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Add_String_String, QArg_Slot{*destSlotIndex}, {QArg_Slot{curSlotIndex}, QArg_Slot{elemSlotIndex}});
            RETURN_ON_ERROR(e_emitResult);
        }
    }

    return {};
}

std::expected<void, DiagPtr> TranslateMInitExp_StringToQInstsWithNewScope(MInitExp_String* exp, std::optional<size_t> destSlot, QTranslationContexts& contexts)
{
    ScopeGuard guard{contexts.bodyContext};
    return TranslateMInitExp_StringToQInsts(exp, destSlot, contexts);
}

} // namespace Citron