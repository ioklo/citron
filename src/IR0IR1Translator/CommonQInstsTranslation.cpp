#include "CommonQInstsTranslation.h"
#include <optional>
#include <ranges>
#include <cassert>

#include "Infra/Variants.h"
#include "Infra/Expected.h"
#include "Infra/Ptr.h"
#include "Logging/Diag.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RFuncDecl.h"
#include "MIR/MExp.h"
#include "MIR/MInitExp.h"
#include "MIR/MLoc.h"
#include "MLocToQInsts.h"
#include "MCreateToQInsts.h"
#include "MReadToQInsts.h"
#include "MStmtToQInsts.h"
#include "QTranslationContexts.h"
#include "QBodyContext.h"
#include "QScopeGuard.h"
#include "QEmitState.h"
#include "QLazyBlock.h"
#include "MLocToQInsts.h"
#include "QFuncInfo.h"
#include "QAbi.h"
#include "MqIntrinsicInfo.h"
#include "MqFactory.h"

using namespace std;

namespace Citron {

namespace {
expected<QEmitState<QLocResult>, DiagPtr> TranslateMInitExp_StringElemToQInsts(MInitExp_StringElem& elem, QTranslationContexts& contexts)
{
    return visit([&contexts](auto& elem) -> expected<QEmitState<QLocResult>, DiagPtr> {
        auto& bodyContext = contexts.bodyContext;
        using T = remove_cvref_t<decltype(elem)>;
        if constexpr (same_as<T, MInitExp_StringElem_Text>)
        {
            auto* stringType = contexts.rFactory->MakeStringType();
            size_t slotIndex = bodyContext.NewSlot(stringType);
            bodyContext.EmitInst(QInst_Ctor_String{QArg_Addr_OfSlot{slotIndex}, elem.text});

            return QLocResult_Slot{slotIndex};
        }
        else if constexpr (same_as<T, MInitExp_StringElem_InitExp>)
        {
            auto* stringType = bodyContext.GetStringType();
            size_t slotIndex = bodyContext.NewSlot(stringType);
            auto e_s_result = TranslateMCreate_NBCToQInsts(elem.initExp, slotIndex, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_result);

            return QLocResult_Slot{slotIndex};
        }
        else if constexpr (same_as<T, MInitExp_StringElem_Loc>)
        {
            auto e_s_locResult = TranslateMLocToQInsts(elem.loc, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_locResult);

            return **e_s_locResult;
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
            bodyContext.EmitInst(QInst_Ctor_String{QArg_Addr_OfSlot{*o_destSlotIndex}, elem.text});
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

            auto locArg = MakeAddrCallArg(**e_s_locResult, contexts);

            contexts.bodyContext.EmitIntrinsic(
                QInst_IntrinsicKind::CopyCtor_Void_StringRef_StringInRef, 
                nullopt, 
                {QArg_CallArg_AddrOfSlot{*o_destSlotIndex}, locArg});

            return QEmitState_Ready{};
        }
        else static_assert(false);
    }, elem);
}

struct HandleCallContext
{
    optional<QArg_Dest> o_dest;
    vector<QArg_CallArg> args;

    HandleCallContext() = default;
    HandleCallContext(const HandleCallContext&) = delete;
    HandleCallContext(HandleCallContext&&) noexcept = default;
};

// return 값에 해당하는 slotIndex (direct, indirect 둘다 동일하게)
optional<size_t> HandleReturn(RType* retType, QReturnPassingMode& retPassingMode, optional<size_t>& o_destSlotIndex, HandleCallContext& callContext, QBodyContext& bodyContext)
{
    return visit([retType, o_destSlotIndex, &callContext, &bodyContext](auto& returnPassingMode) -> optional<size_t> {
        using T = remove_cvref_t<decltype(returnPassingMode)>;
        if constexpr (same_as<T, QReturnPassingMode_Void>)
        {
            callContext.o_dest = nullopt;
            return nullopt;
        }
        else if constexpr (same_as<T, QReturnPassingMode_Direct>)
        {
            // direct는 return 주소를 args에 넣지 않는다
            if (o_destSlotIndex)
                callContext.o_dest = QArg_Dest{*o_destSlotIndex};
            else
                callContext.o_dest = QArg_Dest{bodyContext.NewTempSlot(retType)};

            return callContext.o_dest->index;
        }
        else if constexpr (same_as<T, QReturnPassingMode_Indirect>)
        {
            // ret값을 간접적으로 받는 경우, ret값이 저장될 slot을 하나 만든다
            assert(returnPassingMode.index == callContext.args.size());
            size_t retSlotIndex = o_destSlotIndex ? *o_destSlotIndex : bodyContext.NewTempSlot(retType);
            callContext.args.push_back(QArg_CallArg_AddrOfSlot{retSlotIndex});
            callContext.o_dest = nullopt;
            return retSlotIndex;
        }
        else static_assert(false);
    }, retPassingMode);
}

// o_instance: exp->callable.o_instance
// thisPassingMode: funcInfo.thisPassingMode
expected<QEmitState<void>, DiagPtr> HandleThis(MLoc* o_instance, QThisPassingMode& thisPassingMode, vector<QArg_CallArg>& args, QTranslationContexts& contexts)
{
    return visit([o_instance, &args, &contexts](auto& thisPassingMode) -> expected<QEmitState<void>, DiagPtr> {
        using T = remove_cvref_t<decltype(thisPassingMode)>;
        if constexpr (same_as<T, QThisPassingMode_None>)
        {
            return QEmitState_Ready{};
        }
        else if constexpr (same_as<T, QThisPassingMode_Handle>)
        {
            assert(o_instance);

            // this가 handle타입이다(class C 같은)
            auto e_s_loc = TranslateMLocToQInsts(o_instance, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_loc);

            return visit([o_instance, &args, &contexts](auto& loc) -> expected<QEmitState<void>, DiagPtr> {
                using U = remove_cvref_t<decltype(loc)>;
                if constexpr (same_as<U, QLocResult_Slot>)
                {
                    // this는 slot에 들어있다
                    args.push_back(QArg_CallArg_Slot{loc.slotIndex});
                    return QEmitState_Ready{};
                }
                else if constexpr (same_as<U, QLocResult_Ptr>)
                {
                    auto* type = GetType(o_instance, &*contexts.rFactory);

                    // this는 ptr로 들어있다. Handle이므로, ptr이 가리키는 곳으로부터 loading한다
                    size_t thisSlotIndex = contexts.bodyContext.NewTempSlot(type);
                    contexts.bodyContext.EmitInst(QInst_Load{type, QArg_Dest{thisSlotIndex}, QArg_Addr_PtrSlot{loc.slotIndex}});
                    args.push_back(QArg_CallArg_Slot{thisSlotIndex});
                    return QEmitState_Ready{};
                }
                else static_assert(false);
            }, **e_s_loc);
        }
        else if constexpr (same_as<T, QThisPassingMode_Ptr>)
        {
            assert(o_instance);

            // this가 ref타입이다(struct S& 같은)
            auto e_s_loc = TranslateMLocToQInsts(o_instance, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_loc);

            auto locArg = MakeAddrCallArg(**e_s_loc, contexts);
            args.push_back(locArg);
            return QEmitState_Ready{};
        }
        else static_assert(false);
    }, thisPassingMode);
}

expected<QEmitState<optional<size_t>>, DiagPtr> HandleCallCore(
    RType* retType,
    QFuncInfo& funcInfo,
    optional<size_t>& o_destSlotIndex,
    MLoc* o_instance,
    vector<MArgument>& mArgs,
    HandleCallContext& callContext,
    QTranslationContexts& contexts)
{
    // 1. Return 처리
    auto o_retSlotIndex = HandleReturn(retType, funcInfo.returnPassingMode, o_destSlotIndex, callContext, contexts.bodyContext);

    // 2. This 처리
    auto e_s_thisResult = HandleThis(o_instance, funcInfo.thisPassingMode, callContext.args, contexts);
    RETURN_ON_ERROR_OR_DONE(e_s_thisResult);

    // 3. 인자를 args에 넣는다                
    auto e_s_result = TranslateMArgumentsToQInsts(callContext.args, mArgs, funcInfo, contexts);
    RETURN_ON_ERROR_OR_DONE(e_s_result);

    return o_retSlotIndex;
}


} // namespace 

expected<QEmitState<optional<size_t>>, DiagPtr> HandleIntrinsicCall(
    MqIntrinsicInfo& intrinsicInfo,
    optional<size_t> o_destSlotIndex,
    RTypeArguments* typeArgs,
    vector<MArgument>& mArgs,
    QTranslationContexts& contexts)
{
    auto funcInfo = contexts.qAbi->GetFuncInfo(intrinsicInfo, typeArgs);
    auto* retType = GetType(intrinsicInfo.funcRet, &*contexts.rFactory);
    HandleCallContext callContext{};
    auto e_s_o_retSlotIndex = HandleCallCore(retType, funcInfo, o_destSlotIndex, nullptr, mArgs, callContext, contexts);
    RETURN_ON_ERROR_OR_DONE(e_s_o_retSlotIndex);

    contexts.bodyContext.EmitIntrinsic(intrinsicInfo.kind, callContext.o_dest, move(callContext.args));
    return **e_s_o_retSlotIndex;
}

expected<QEmitState<optional<size_t>>, DiagPtr> HandleCall(
    RFuncDecl* decl,
    RTypeArguments* typeArgs,
    optional<size_t> o_destSlotIndex,
    MLoc* o_instance,
    vector<MArgument>& mArgs,
    QTranslationContexts& contexts)
{
    // generics는 어떻게 하나요
    // T F<T>(T t) { return t; }
    // Generics는 T에 관한 정보를 더 넘겨준다 (크기 등)
    // 따라서 이 함수는 t, {F함수에 대한 constraint table} 두 인자를 받는다
    // 그리고 t는 항상 stack pointer를 가리키게 된다 (callee쪽에서 크기를 정확히 알 수 없으므로)

    auto funcInfo = contexts.qAbi->GetFuncInfo(decl, typeArgs); // TODO: [62] Generics 구현
    auto* retType = decl->GetReturnType(typeArgs);

    HandleCallContext callContext{};
    auto e_s_o_retSlotIndex = HandleCallCore(retType, funcInfo, o_destSlotIndex, o_instance, mArgs, callContext, contexts);
    RETURN_ON_ERROR_OR_DONE(e_s_o_retSlotIndex);
    contexts.bodyContext.EmitInst(QInst_Call{decl, callContext.o_dest, move(callContext.args)});

    return **e_s_o_retSlotIndex;
}

QArg_CallArg MakeAddrCallArg(QLocResult& locResult, QTranslationContexts& contexts)
{
    return visit([](auto& locResult) -> QArg_CallArg {
        using T = remove_cvref_t<decltype(locResult)>;
        if constexpr (same_as<T, QLocResult_Slot>) return QArg_CallArg_AddrOfSlot{locResult.slotIndex};
        else if constexpr (same_as<T, QLocResult_Ptr>) return QArg_CallArg_Slot{locResult.slotIndex};
        else static_assert(false);
    }, locResult);
}

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
            bodyContext.EmitInst(QInst_AddrOf{QArg_Dest{destSlotIndex}, readResult.slotIndex});

            return destSlotIndex;
        }
        else if constexpr (same_as<T, QLocResult_Ptr>) return readResult.slotIndex;
        else static_assert(false);
    }, locResult);
}

expected<QEmitState<QArg_CallArg>, DiagPtr> TranslateMArgumentToQInsts(MArgument& arg, QParamPassingMode passingMode, QTranslationContexts& contexts)
{
    return visit([passingMode, &contexts](auto& arg) -> expected<QEmitState<QArg_CallArg>, DiagPtr> {
        using T = remove_cvref_t<decltype(arg)>;

        if constexpr (same_as<T, MArgument_Create>)
        {
            if (passingMode == QParamPassingMode::Direct)
            {
                // argument를 위한 slot을 하나 마련한다
                auto* argType = GetType(arg.create, &*contexts.rFactory);
                auto argSlotIndex = contexts.bodyContext.NewSlot(argType);

                auto e_s_result = TranslateMCreateToQInsts(arg.create, argSlotIndex, contexts);
                RETURN_ON_ERROR_OR_DONE(e_s_result);

                return QArg_CallArg_Slot{argSlotIndex};
            }
            else if (passingMode == QParamPassingMode::Indirect)
            {
                // argument를 위한 slot을 하나 마련한다
                auto* argType = GetType(arg.create, &*contexts.rFactory);
                auto argSlotIndex = contexts.bodyContext.NewSlot(argType);
                auto e_s_result = TranslateMCreateToQInsts(arg.create, argSlotIndex, contexts);
                RETURN_ON_ERROR_OR_DONE(e_s_result);
                
                return QArg_CallArg_AddrOfSlot{argSlotIndex};
            }
            else throw NotImplementedException{};
        }
        else if constexpr (same_as<T, MArgument_Loc>) // loc은 pointer로 넘긴다
        {
            assert(passingMode == QParamPassingMode::Ref);

            auto e_s_locResult = TranslateMLocToQInsts(arg.loc, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_locResult);

            return MakeAddrCallArg(**e_s_locResult, contexts);
        }
        else if constexpr (same_as<T, MArgument_Move>)
        {
            assert(passingMode == QParamPassingMode::Ref);

            // TODO: [30] move구현
            throw NotImplementedException{};
        }
        else if constexpr (same_as<T, MArgument_Forward>)
        {
            assert(passingMode == QParamPassingMode::Forward);

            // TODO: [57] [forward] ref parameter 지원
            throw NotImplementedException{};
        }
        else if constexpr (same_as<T, MArgument_Params>) // 파라미터를 여러개 받는 경우
        {
            assert(passingMode == QParamPassingMode::Params);
            // TODO: [31] params 구현
            throw NotImplementedException{};
        }
        else static_assert(false);
    }, arg);
}

expected<QEmitState<void>, DiagPtr> TranslateMArgumentsToQInsts(vector<QArg_CallArg>& qArgs, vector<MArgument>& mArgs, QFuncInfo& funcInfo, QTranslationContexts& contexts)
{
    assert(qArgs.size() == funcInfo.explicitArgStartIndex);
    assert(mArgs.size() == funcInfo.paramPassingModes.size());

    size_t count = mArgs.size();
    qArgs.reserve(qArgs.size() + count);
    for (size_t i = 0; i < count; i++)
    {   
        auto e_s_qArg = TranslateMArgumentToQInsts(mArgs[i], funcInfo.paramPassingModes[i], contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_qArg);

        qArgs.push_back(move(**e_s_qArg));
    }

    return QEmitState_Ready{};
}

void HandleReturn(optional<QArg_Dest>& o_retSlotIndex, std::vector<QArg_CallArg>& qArgs, QReturnPassingMode& retPassingMode, QBodyContext& bodyContext)
{
    visit([&o_retSlotIndex, &qArgs, &bodyContext](auto& retPassingMode) {
        using T = remove_cvref_t<decltype(retPassingMode)>;
        if constexpr (same_as<T, QReturnPassingMode_Void>)
        {
            o_retSlotIndex.reset();
        }
        else if constexpr (same_as<T, QReturnPassingMode_Direct>)
        {
            // use retSlotIndex
            auto* stringType = bodyContext.GetStringType();
            size_t slotIndex = bodyContext.NewSlot(stringType);
            o_retSlotIndex.emplace(QArg_Dest{slotIndex});
        }
        else if constexpr (same_as<T, QReturnPassingMode_Indirect>)
        {
            o_retSlotIndex.reset();

            auto* stringType = bodyContext.GetStringType();
            size_t slotIndex = bodyContext.NewSlot(stringType);
            qArgs.push_back(QArg_CallArg_AddrOfSlot{slotIndex});
        }
        else static_assert(false);
    }, retPassingMode);
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
        auto e_s_front = TranslateMInitExp_StringElemToQInsts(exp->elements.front(), contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_front);

        auto& addIntrinsicInfo = contexts.mqFactory->GetIntrinsicInfo(QInst_IntrinsicKind::Add_String_StringInRef_StringInRef);
        auto addFuncInfo = contexts.qAbi->GetFuncInfo(addIntrinsicInfo, contexts.rFactory->MakeEmptyTypeArguments());

        vector<QArg_CallArg> args;
        QArg_CallArg curArg = MakeAddrCallArg(**e_s_front, contexts);
        assert(holds_alternative<QThisPassingMode_None>(addFuncInfo.thisPassingMode));

        for (size_t i = 1, end = exp->elements.size() - 1; i < end; i++)
        {   
            auto e_s_elemLoc = TranslateMInitExp_StringElemToQInsts(exp->elements[i], contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_elemLoc);

            // 리턴값, 파라미터 넘기는 방식을 조정할 필요가 없다. 
            // string은 NBC고 NBC 리턴은 무조건 첫번째 인자로 리턴 위치를 넘기도록 되어있다

            // abi가 string을 

            auto elemArg = MakeAddrCallArg(**e_s_elemLoc, contexts);

            auto newSlotIndex = bodyContext.NewSlot(stringType);
            QArg_CallArg_AddrOfSlot newArg{newSlotIndex};

            bodyContext.EmitIntrinsic(
                QInst_IntrinsicKind::Add_String_StringInRef_StringInRef,
                nullopt, // string은 dest는 nullopt, 첫번째 인자로 ptr을 넣는다
                {newArg, curArg, elemArg});

            curArg = newArg;
        }
        
        auto e_s_backLoc = TranslateMInitExp_StringElemToQInsts(exp->elements.back(), contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_backLoc);

        if (o_destSlotIndex)
        {
            auto backArg = MakeAddrCallArg(**e_s_backLoc, contexts);

            contexts.bodyContext.EmitIntrinsic(
                QInst_IntrinsicKind::Add_String_StringInRef_StringInRef,
                nullopt,
                {QArg_CallArg_AddrOfSlot{*o_destSlotIndex}, curArg, backArg});
        }
    }

    return QEmitState_Ready{};
}

expected<QEmitState<void>, DiagPtr> HandleInlineBlock(MStmt_Scope* scope, optional<size_t> o_destSlotIndex, QTranslationContexts& contexts)
{
    // inlineBlock을 destSlot없이 호출할 일이 없도록 해야한다. 
    // 보통은 MStmt_Exp에서 호출될텐데, 여기는 Call, Assign만 가능하므로 괜찮다
    if (!o_destSlotIndex) throw RuntimeFatalException{};

    // 지금 블록 말고 leaveBlock을 하나 더 만들어야 할거 같다
    auto lazyExitBlock = MakePtr<QLazyBlock>("inline_exit");
    auto e_s_result = TranslateMStmt_ScopeToQInsts_Inline(scope, lazyExitBlock, *o_destSlotIndex, contexts);
    RETURN_ON_ERROR(e_s_result);

    // inline block이 Ready면 이상한거다
    if (*e_s_result)
        return Error<Error_InlineExp_ShouldLeaveWithValue>();

    // 내부에서 exitBlock을 쓰지 않았다면 Done이다
    if (!lazyExitBlock->HasBlock())
    {
        return QEmitState_Done{};
    }
    else
    {
        auto* exitBlock = lazyExitBlock->GetBlock(&contexts.bodyContext);
        contexts.bodyContext.SetCurBlock(exitBlock);
        return QEmitState_Ready{};
    }
}

} // namespace Citron