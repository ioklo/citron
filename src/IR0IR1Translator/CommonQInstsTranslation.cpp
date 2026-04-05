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
#include "QScopeGuard.h"
#include "MLocToQInsts.h"
#include "MCreateToQInsts.h"
#include "MReadToQInsts.h"
#include "QEmitState.h"

using namespace std;

namespace Citron {

namespace {

size_t MakePtrSlot(QLocResult& locResult, QTranslationContexts& contexts)
{
    return visit([&contexts](auto& readResult)-> size_t {
        auto& bodyContext = contexts.bodyContext;
        using T = remove_cvref_t<decltype(readResult)>;
        if constexpr (same_as<T, QLocResult_Slot>)
        {
            auto* stringType = bodyContext.GetStringType();
            auto* stringPtrType = bodyContext.GetPtrType(stringType);
            auto destSlotIndex = bodyContext.NewSlot(stringPtrType);
            bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{destSlotIndex}, QArg_Slot{readResult.slotIndex}});
            
            return destSlotIndex;
        }
        else if constexpr (same_as<T, QLocResult_Ptr>) return readResult.slotIndex;
        else static_assert(false);
    }, locResult);
}

// ptr slot만 반환하도록 한다
expected<QEmitState<QReadResult_Ptr>, DiagPtr> TranslateMInitExp_StringElemToQInsts(MInitExp_StringElem& elem, QTranslationContexts& contexts)
{
    return visit([&contexts](auto& elem) -> expected<QEmitState<QReadResult_Ptr>, DiagPtr> {
        auto& bodyContext = contexts.bodyContext;
        using T = remove_cvref_t<decltype(elem)>;
        if constexpr (same_as<T, MInitExp_StringElem_Text>)
        {
            auto* stringType = contexts.rFactory->MakeStringType();
            auto* stringPtrType = contexts.rFactory->MakePtrType(stringType);

            size_t slotIndex = bodyContext.NewSlot(stringType);
            size_t ptrSlotIndex = bodyContext.NewSlot(stringPtrType);

            bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{ptrSlotIndex}, QArg_Slot{slotIndex}});
            bodyContext.EmitInst(QInst_Ctor_String{QArg_Slot{ptrSlotIndex}, elem.text});

            return QReadResult_Ptr{ptrSlotIndex};
        }
        else if constexpr (same_as<T, MInitExp_StringElem_InitExp>)
        {
            auto* stringType = bodyContext.GetStringType();
            size_t slotIndex = bodyContext.NewSlot(stringType);
            auto e_s_result = TranslateMCreate_NBCToQInsts(elem.initExp, slotIndex, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_result);

            auto* stringPtrType = bodyContext.GetPtrType(stringType);
            size_t ptrSlotIndex = bodyContext.NewSlot(stringPtrType);
            bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{ptrSlotIndex}, QArg_Slot{slotIndex}});

            return QReadResult_Ptr{ptrSlotIndex};
        }
        else if constexpr (same_as<T, MInitExp_StringElem_Loc>)
        {
            auto e_s_locResult = TranslateMLocToQInsts(elem.loc, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_locResult);

            auto ptrSlotIndex = MakePtrSlot(**e_s_locResult, contexts);
            return QReadResult_Ptr{ptrSlotIndex};
        }
        
        else static_assert(false);
    }, elem);
}

expected<QEmitState<void>, DiagPtr> TranslateMInitExp_StringElemToQInstsForCreate(MInitExp_StringElem& elem, optional<size_t> o_destSlotIndex, QTranslationContexts& contexts)
{
    if (!o_destSlotIndex) return QEmitState_Ready{};

    return visit([&o_destSlotIndex, &contexts](auto& elem) -> expected<QEmitState<void>, DiagPtr> {
        using T = remove_cvref_t<decltype(elem)>;
        if constexpr (same_as<T, MInitExp_StringElem_Text>)
        {
            auto& bodyContext = contexts.bodyContext;

            size_t ptrSlotIndex = bodyContext.NewSlot(bodyContext.GetPtrType(bodyContext.GetStringType())); // string slot하나 만들어서 ctor의 this로 쓴다
            bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{ptrSlotIndex}, QArg_Slot{*o_destSlotIndex}});
            bodyContext.EmitInst(QInst_Ctor_String{QArg_Slot{ptrSlotIndex}, elem.text});
            return QEmitState_Ready{};
        }
        else if constexpr (same_as<T, MInitExp_StringElem_InitExp>)
        {   
            auto e_s_result = TranslateMCreate_NBCToQInsts(elem.initExp, o_destSlotIndex, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_result);
            return QEmitState_Ready{};
        }
        else if constexpr (same_as<T, MInitExp_StringElem_Loc>)
        {
            auto e_s_locResult = TranslateMLocToQInsts(elem.loc, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_locResult);

            size_t ptrSlotIndex = MakePtrSlot(**e_s_locResult, contexts);

            contexts.bodyContext.EmitIntrinsic(
                QInst_IntrinsicKind::CopyCtor_StringPtr_StringPtr_Void, 
                nullopt, 
                {QArg_Slot{*o_destSlotIndex}, QArg_Slot{ptrSlotIndex}});

            return QEmitState_Ready{};
        }
        else static_assert(false);
    }, elem);
}
} // namespace 

expected<QEmitState<QArg_Input>, DiagPtr> TranslateMArgumentToQInsts(MArgument& arg, QTranslationContexts& contexts)
{
    return visit([&contexts](auto& arg) -> expected<QEmitState<QArg_Input>, DiagPtr> {
        using T = remove_cvref_t<decltype(arg)>;

        if constexpr (same_as<T, MArgument_Create>)
        {
            // argument를 위한 slot을 하나 마련한다
            auto* argType = GetType(arg.create, &*contexts.rFactory);
            auto argSlotIndex = contexts.bodyContext.NewSlot(argType);

            auto e_s_result = TranslateMCreateToQInsts(arg.create, argSlotIndex, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_result);

            return QArg_Slot{argSlotIndex};
        }
        else if constexpr (same_as<T, MArgument_Loc>) // loc은 pointer로 넘긴다
        {
            auto e_s_locResult = TranslateMLocToQInsts(arg.loc, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_locResult);

            return visit([&contexts](auto& locResult) -> expected<QEmitState<QArg_Input>, DiagPtr> {
                using U = remove_cvref_t<decltype(locResult)>;

                if constexpr (same_as<U, QLocResult_Slot>)
                {
                    // slot이면 addrOf를 써서 ptr로 만든다
                    auto* rPtrType = contexts.bodyContext.GetPtrType();
                    auto ptrSlotIndex = contexts.bodyContext.NewSlot(rPtrType);

                    contexts.bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{ptrSlotIndex}, QArg_Slot{locResult.slotIndex}});
                    return QArg_Slot{ptrSlotIndex};
                }
                else if constexpr (same_as<U, QLocResult_Ptr>)
                {
                    // ptr이면 그대로 넣어준다
                    return QArg_Slot{locResult.slotIndex};
                }
                else static_assert(false);
            }, **e_s_locResult);
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

expected<QEmitState<vector<QArg_Input>>, DiagPtr> TranslateMArgumentsToQInsts(vector<MArgument>& mArgs, QTranslationContexts& contexts)
{
    vector<QArg_Input> qArgs;
    qArgs.reserve(mArgs.size());

    for (auto& mArg : mArgs)
    {
        auto e_s_qArg = TranslateMArgumentToQInsts(mArg, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_qArg);

        qArgs.push_back(move(**e_s_qArg));
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
            {MExp_CallIntrinsicKind::LessThan_StringPtr_StringPtr_Bool, {.kind = QInst_IntrinsicKind::LessThan_StringPtr_StringPtr_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::GreaterThan_Int_Int_Bool, {.kind = QInst_IntrinsicKind::GreaterThan_Int_Int_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::GreaterThan_StringPtr_StringPtr_Bool, {.kind = QInst_IntrinsicKind::GreaterThan_StringPtr_StringPtr_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::LessThanOrEqual_Int_Int_Bool, {.kind = QInst_IntrinsicKind::LessThanOrEqual_Int_Int_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::LessThanOrEqual_StringPtr_StringPtr_Bool, {.kind = QInst_IntrinsicKind::LessThanOrEqual_StringPtr_StringPtr_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::GreaterThanOrEqual_Int_Int_Bool, {.kind = QInst_IntrinsicKind::GreaterThanOrEqual_Int_Int_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::GreaterThanOrEqual_StringPtr_StringPtr_Bool, {.kind = QInst_IntrinsicKind::GreaterThanOrEqual_StringPtr_StringPtr_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::Equal_Int_Int_Bool, {.kind = QInst_IntrinsicKind::Equal_Int_Int_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::Equal_Bool_Bool_Bool, {.kind = QInst_IntrinsicKind::Equal_Bool_Bool_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::Equal_StringPtr_StringPtr_Bool, {.kind = QInst_IntrinsicKind::Equal_StringPtr_StringPtr_Bool, .type = boolType}},
            {MExp_CallIntrinsicKind::GetIterator_ListPtr_ListIterator, {.kind = QInst_IntrinsicKind::GetIterator_ListPtr_ListIterator, .type = nullptr}} // TODO:
        };
        }();

    auto i = m.find(kind);
    if (i == m.end()) return nullptr;

    return &i->second;
}

QIntrinsicInfo* GetIntrinsicInfo(MInitExp_CallIntrinsicKind kind, QTranslationContexts& contexts)
{
    static unordered_map<MInitExp_CallIntrinsicKind, QIntrinsicInfo> m = [&contexts]() {
        auto* boolType = contexts.rFactory->MakeBoolType();
        auto* intType = contexts.rFactory->MakeIntType();
        auto* stringType = contexts.rFactory->MakeStringType();

        return unordered_map<MInitExp_CallIntrinsicKind, QIntrinsicInfo>{
            {MInitExp_CallIntrinsicKind::ToString_Bool_String, {.kind= QInst_IntrinsicKind::ToString_Bool_String, .type = stringType}},
            {MInitExp_CallIntrinsicKind::ToString_Int_String, {.kind= QInst_IntrinsicKind::ToString_Int_String, .type = stringType}},
            {MInitExp_CallIntrinsicKind::Add_String_String_String, {.kind= QInst_IntrinsicKind::Add_StringPtr_StringPtr_String, .type = stringType}},
        };
    }();

    auto i = m.find(kind);
    if (i == m.end()) return nullptr;

    return &i->second;
}

expected<QEmitState<void>, DiagPtr> TranslateMInitExp_StringToQInsts(MInitExp_String* exp, optional<size_t> o_destSlotIndex, QTranslationContexts& contexts)
{
    auto& bodyContext = contexts.bodyContext;
    assert(!exp->elements.empty());

    // 원소가 한개라면, dest에 직접 넣는다
    if (exp->elements.size() == 1)
    {
        auto e_s_result = TranslateMInitExp_StringElemToQInstsForCreate(exp->elements.front(), o_destSlotIndex, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_result);
    }
    else
    {
        auto* stringType = bodyContext.GetStringType();
        auto* stringPtrType = bodyContext.GetPtrType(stringType);


        // "abc $x" => "abc " + x
        auto e_s_frontPtr = TranslateMInitExp_StringElemToQInsts(exp->elements.front(), contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_frontPtr);

        size_t curPtrSlotIndex = (*e_s_frontPtr)->slotIndex;
        for (size_t i = 1, end = exp->elements.size() - 1; i < end; i++)
        {   
            auto e_s_elemPtr = TranslateMInitExp_StringElemToQInsts(exp->elements[i], contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_elemPtr);
            size_t elemPtrSlotIndex = (*e_s_elemPtr)->slotIndex;

            auto newSlotIndex = bodyContext.NewSlot(stringType);
            bodyContext.EmitIntrinsic(
                QInst_IntrinsicKind::Add_StringPtr_StringPtr_String, 
                QArg_Slot{newSlotIndex}, 
                {QArg_Slot{curPtrSlotIndex}, QArg_Slot{elemPtrSlotIndex}});

            auto newPtrSlotIndex = bodyContext.NewSlot(stringPtrType);
            bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{newPtrSlotIndex}, QArg_Slot{newSlotIndex}});

            curPtrSlotIndex = newPtrSlotIndex;
        }
        
        auto e_s_backPtr = TranslateMInitExp_StringElemToQInsts(exp->elements.back(), contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_backPtr);

        if (o_destSlotIndex)
        {
            contexts.bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Add_StringPtr_StringPtr_String, 
                QArg_Slot{*o_destSlotIndex}, {QArg_Slot{curPtrSlotIndex}, QArg_Slot{(*e_s_backPtr)->slotIndex}});
        }
    }

    return QEmitState_Ready{};
}

} // namespace Citron