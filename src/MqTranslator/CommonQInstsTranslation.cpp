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
#include "MReadToQInsts.h"
#include "MStmtToQInsts.h"
#include "MqTranslationContexts.h"
#include "MqBodyContext.h"
#include "MqScopeGuard.h"
#include "MqEmitState.h"
#include "MqLazyBlock.h"
#include "MLocToQInsts.h"
#include "MqFuncInfo.h"
#include "MqAbi.h"
#include "MqIntrinsicInfo.h"
#include "MqFactory.h"
#include "MqCreateTarget.h"
#include "MExpToQInsts.h"
#include "MCreateToQInsts.h"
#include "MInitExpToQInsts.h"

using namespace std;

namespace Citron {

namespace {
expected<MqEmitState<MqLocResult>, DiagPtr> TranslateMInitExp_StringElemToQInsts(MInitExp_StringElem& elem, MqTranslationContexts& contexts)
{
    return visit([&contexts](auto& elem) -> expected<MqEmitState<MqLocResult>, DiagPtr> {
        auto& bodyContext = contexts.bodyContext;
        using T = remove_cvref_t<decltype(elem)>;
        if constexpr (same_as<T, MInitExp_StringElem_Text>)
        {
            auto* stringType = contexts.rFactory->MakeStringType();
            size_t slotIndex = bodyContext.AddTemp(stringType, "stringElem");
            bodyContext.EmitInst(QInst_Ctor_String{QArg_Addr_OfSlot{slotIndex}, elem.text});

            return MqLocResult_Slot{slotIndex};
        }
        else if constexpr (same_as<T, MInitExp_StringElem_InitExp>)
        {
            auto* stringType = bodyContext.GetStringType();
            size_t slotIndex = bodyContext.AddTemp(stringType, "stringElem");
            auto e_s_result = TranslateMInitExpToQInsts(elem.initExp, MqCreateTarget_Slot{slotIndex}, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_result);

            return MqLocResult_Slot{slotIndex};
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

expected<MqEmitState<void>, DiagPtr> TranslateMInitExp_StringElemToQInstsForCreate(MInitExp_StringElem& elem, MqCreateTarget createTarget, MqTranslationContexts& contexts)
{
    return visit([&createTarget, &contexts](auto& elem) -> expected<MqEmitState<void>, DiagPtr> {

        using T = remove_cvref_t<decltype(elem)>;
        if constexpr (same_as<T, MInitExp_StringElem_Text>)
        {
            if (auto o_qArgAddr = MakeQArg_Addr(createTarget, contexts))
            {
                auto& bodyContext = contexts.bodyContext;
                bodyContext.EmitInst(QInst_Ctor_String{*o_qArgAddr, elem.text});
            }
            return MqEmitState_Ready{};
        }
        else if constexpr (same_as<T, MInitExp_StringElem_InitExp>)
        {
            auto e_s_result = TranslateMInitExpToQInsts(elem.initExp, createTarget, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_result);
            return MqEmitState_Ready{};
        }
        else if constexpr (same_as<T, MInitExp_StringElem_Loc>)
        {
            auto e_s_locResult = TranslateMLocToQInsts(elem.loc, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_locResult);

            if (auto o_createTargetAddrArg = MakeAddrCallArg(createTarget, contexts))
            {
                auto locArg = MakeAddrCallArg(**e_s_locResult, contexts);

                contexts.bodyContext.EmitIntrinsic(
                    QInst_IntrinsicKind::CopyCtor_Void_StringRef_StringInRef,
                    nullopt,
                    {*o_createTargetAddrArg, locArg});
            }

            return MqEmitState_Ready{};
        }
        else static_assert(false);
    }, elem);
}

struct HandleCallContext
{
    optional<QArg_Dest> o_dest;
    vector<QArg_CallArg> args;
    optional<QInst> o_postCallInst;

    HandleCallContext() = default;
    HandleCallContext(const HandleCallContext&) = delete;
    HandleCallContext(HandleCallContext&&) noexcept = default;
};

// indirect return 값에 해당하는 slotIndex (direct, indirect 둘다 동일하게)
// 이 함수의 리턴은 MRead로써 Call을 할 경우, 결괏값의 위치를 나타낸다. QLocResult는 DirectReturn을 가리킬 수 없으므로, DirectReturn인 경우에는 nullopt을 반환한다
optional<MqLocResult> HandleReturn(RType* retType, MqReturnPassingMode& retPassingMode, MqCreateTarget createTarget, HandleCallContext& callContext, MqBodyContext& bodyContext)
{
    return visit([retType, createTarget, &callContext, &bodyContext](auto& returnPassingMode) -> optional<MqLocResult> {
        using T = remove_cvref_t<decltype(returnPassingMode)>;
        if constexpr (same_as<T, MqReturnPassingMode_Void>)
        {
            callContext.o_dest = nullopt;
            return nullopt;
        }
        else if constexpr (same_as<T, MqReturnPassingMode_Direct>)
        {
            return visit([retType, &callContext, &bodyContext](auto& createTarget) -> optional<MqLocResult> {
                using U = remove_cvref_t<decltype(createTarget)>;
                if constexpr (same_as<U, MqCreateTarget_Discard>)
                {
                    size_t tempRetSlotIndex = bodyContext.AddTemp(retType, "ret");
                    callContext.o_dest = QArg_Dest_Slot{tempRetSlotIndex};
                    return MqLocResult_Slot{tempRetSlotIndex};
                }
                else if constexpr (same_as<U, MqCreateTarget_Slot>)
                {
                    callContext.o_dest = QArg_Dest_Slot{createTarget.slotIndex};
                    return MqLocResult_Slot{createTarget.slotIndex};
                }
                else if constexpr (same_as<U, MqCreateTarget_Ptr>)
                {
                    size_t tempRetSlotIndex = bodyContext.AddTemp(retType, "ret");
                    callContext.o_dest = QArg_Dest_Slot{tempRetSlotIndex};
                    callContext.o_postCallInst = QInst_Store{retType, QArg_Addr_PtrSlot{createTarget.slotIndex}, QArg_Value_Slot{tempRetSlotIndex}};
                    return MqLocResult_Ptr{createTarget.slotIndex};
                }
                else if constexpr (same_as<U, MqCreateTarget_DirectReturn>)
                {
                    callContext.o_dest = QArg_Dest_DirectReturn{};
                    return nullopt;
                }
            }, createTarget);
        }
        else if constexpr (same_as<T, MqReturnPassingMode_Indirect>)
        {
            return visit([retType, &returnPassingMode, &callContext, &bodyContext](auto& createTarget) -> optional<MqLocResult> {
                using U = remove_cvref_t<decltype(createTarget)>;

                if constexpr (same_as<U, MqCreateTarget_Discard>)
                {
                    callContext.o_dest = nullopt;

                    size_t tempRetSlotIndex = bodyContext.AddTemp(retType, "ret");                    
                    assert(returnPassingMode.index == callContext.args.size());
                    callContext.args.push_back(QArg_CallArg_AddrOfSlot{tempRetSlotIndex});

                    return MqLocResult_Slot{tempRetSlotIndex};
                }
                else if constexpr (same_as<U, MqCreateTarget_Slot>)
                {
                    callContext.o_dest = nullopt;

                    assert(returnPassingMode.index == callContext.args.size());
                    callContext.args.push_back(QArg_CallArg_AddrOfSlot{createTarget.slotIndex});

                    return MqLocResult_Slot{createTarget.slotIndex};
                }
                else if constexpr (same_as<U, MqCreateTarget_Ptr>)
                {
                    callContext.o_dest = nullopt;

                    assert(returnPassingMode.index == callContext.args.size());
                    callContext.args.push_back(QArg_CallArg_Slot{createTarget.slotIndex});

                    return MqLocResult_Ptr{createTarget.slotIndex};
                }
                else if constexpr (same_as<U, MqCreateTarget_DirectReturn>)
                {   
                    throw RuntimeFatalException{};
                }
                else static_assert(false);
            }, createTarget);
        }
        else static_assert(false);
    }, retPassingMode);
}

// o_instance: exp->callable.o_instance
// thisPassingMode: funcInfo.thisPassingMode
expected<MqEmitState<void>, DiagPtr> HandleThis(MLoc* o_instance, MqThisPassingMode& thisPassingMode, vector<QArg_CallArg>& args, MqTranslationContexts& contexts)
{
    return visit([o_instance, &args, &contexts](auto& thisPassingMode) -> expected<MqEmitState<void>, DiagPtr> {
        using T = remove_cvref_t<decltype(thisPassingMode)>;
        if constexpr (same_as<T, MqThisPassingMode_None>)
        {
            return MqEmitState_Ready{};
        }
        else if constexpr (same_as<T, MqThisPassingMode_Handle>)
        {
            assert(o_instance);

            // this가 handle타입이다(class C 같은)
            auto e_s_loc = TranslateMLocToQInsts(o_instance, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_loc);

            return visit([o_instance, &args, &contexts](auto& loc) -> expected<MqEmitState<void>, DiagPtr> {
                using U = remove_cvref_t<decltype(loc)>;
                if constexpr (same_as<U, MqLocResult_Slot>)
                {
                    // this는 slot에 들어있다
                    args.push_back(QArg_CallArg_Slot{loc.slotIndex});
                    return MqEmitState_Ready{};
                }
                else if constexpr (same_as<U, MqLocResult_Ptr>)
                {
                    auto* type = GetType(o_instance, &*contexts.rFactory);

                    // this는 ptr로 들어있다. Handle이므로, ptr이 가리키는 곳으로부터 loading한다
                    size_t thisSlotIndex = contexts.bodyContext.AddTemp(type, "this");
                    contexts.bodyContext.EmitInst(QInst_Load{type, QArg_Dest_Slot{thisSlotIndex}, QArg_Addr_PtrSlot{loc.slotIndex}});
                    args.push_back(QArg_CallArg_Slot{thisSlotIndex});
                    return MqEmitState_Ready{};
                }
                else static_assert(false);
            }, **e_s_loc);
        }
        else if constexpr (same_as<T, MqThisPassingMode_Ptr>)
        {
            assert(o_instance);

            // this가 ref타입이다(struct S& 같은)
            auto e_s_loc = TranslateMLocToQInsts(o_instance, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_loc);

            auto locArg = MakeAddrCallArg(**e_s_loc, contexts);
            args.push_back(locArg);
            return MqEmitState_Ready{};
        }
        else static_assert(false);
    }, thisPassingMode);
}

expected<MqEmitState<optional<MqLocResult>>, DiagPtr> HandleCallCore(
    RType* retType,
    MqFuncInfo& funcInfo,
    MqCreateTarget createTarget,
    MLoc* o_instance,
    vector<MArgument>& mArgs,
    HandleCallContext& callContext,
    MqTranslationContexts& contexts)
{
    // 1. Return 처리
    auto o_retLocResult = HandleReturn(retType, funcInfo.returnPassingMode, createTarget, callContext, contexts.bodyContext);

    // 2. This 처리
    auto e_s_thisResult = HandleThis(o_instance, funcInfo.thisPassingMode, callContext.args, contexts);
    RETURN_ON_ERROR_OR_DONE(e_s_thisResult);

    // 3. 인자를 args에 넣는다                
    auto e_s_result = TranslateMArgumentsToQInsts(callContext.args, mArgs, funcInfo, contexts);
    RETURN_ON_ERROR_OR_DONE(e_s_result);

    return o_retLocResult;
}


} // namespace 

expected<MqEmitState<optional<MqLocResult>>, DiagPtr> HandleIntrinsicCall(
    MqIntrinsicInfo& intrinsicInfo,
    MqCreateTarget createTarget,
    RTypeArguments* typeArgs,
    vector<MArgument>& mArgs,
    MqTranslationContexts& contexts)
{
    auto funcInfo = contexts.abi->GetFuncInfo(intrinsicInfo, typeArgs);
    auto* retType = GetType(intrinsicInfo.funcRet, &*contexts.rFactory);
    HandleCallContext callContext{};
    auto e_s_o_retLocResult = HandleCallCore(retType, funcInfo, createTarget, nullptr, mArgs, callContext, contexts);
    RETURN_ON_ERROR_OR_DONE(e_s_o_retLocResult);

    contexts.bodyContext.EmitIntrinsic(intrinsicInfo.kind, callContext.o_dest, move(callContext.args));
    if (callContext.o_postCallInst)
        contexts.bodyContext.EmitInst(move(*callContext.o_postCallInst));

    return **e_s_o_retLocResult;
}

expected<MqEmitState<optional<MqLocResult>>, DiagPtr> HandleCall(
    RFuncDecl& decl,
    RTypeArguments* typeArgs,
    MqCreateTarget createTarget,
    MLoc* o_instance,
    vector<MArgument>& mArgs,
    MqTranslationContexts& contexts)
{
    // generics는 어떻게 하나요
    // T F<T>(T t) { return t; }
    // Generics는 T에 관한 정보를 더 넘겨준다 (크기 등)
    // 따라서 이 함수는 t, {F함수에 대한 constraint table} 두 인자를 받는다
    // 그리고 t는 항상 stack pointer를 가리키게 된다 (callee쪽에서 크기를 정확히 알 수 없으므로)

    auto funcInfo = contexts.abi->GetFuncInfo(decl, typeArgs); // TODO: [62] Generics 구현
    auto* retType = GetReturnType(decl, typeArgs);

    HandleCallContext callContext{};
    auto e_s_o_retLocResult = HandleCallCore(retType, funcInfo, createTarget, o_instance, mArgs, callContext, contexts);
    RETURN_ON_ERROR_OR_DONE(e_s_o_retLocResult);
    contexts.bodyContext.EmitInst(QInst_Call{decl, callContext.o_dest, move(callContext.args)});
    if (callContext.o_postCallInst)
        contexts.bodyContext.EmitInst(move(*callContext.o_postCallInst));

    return **e_s_o_retLocResult;
}

QArg_CallArg MakeAddrCallArg(MqLocResult& locResult, MqTranslationContexts& contexts)
{
    return visit([](auto& locResult) -> QArg_CallArg {
        using T = remove_cvref_t<decltype(locResult)>;
        if constexpr (same_as<T, MqLocResult_Slot>) return QArg_CallArg_AddrOfSlot{locResult.slotIndex};
        else if constexpr (same_as<T, MqLocResult_Ptr>) return QArg_CallArg_Slot{locResult.slotIndex};
        else static_assert(false);
    }, locResult);
}

// NBC일때만 가능
optional<QArg_CallArg> MakeAddrCallArg(MqCreateTarget& createTarget, MqTranslationContexts& contexts)
{
    return visit([](auto& createTarget) -> optional<QArg_CallArg> {
        using T = remove_cvref_t<decltype(createTarget)>;
        if constexpr (same_as<T, MqCreateTarget_Discard>)
            return nullopt;
        else if constexpr (same_as<T, MqCreateTarget_Slot>)
            return QArg_CallArg_AddrOfSlot{createTarget.slotIndex};
        else if constexpr (same_as<T, MqCreateTarget_Ptr>)
            return QArg_CallArg_Slot{createTarget.slotIndex};
        else if constexpr (same_as<T, MqCreateTarget_DirectReturn>)
            throw RuntimeFatalException{};
        else static_assert(false);
    }, createTarget);
}

// NBC일때만 가능
optional<QArg_Addr> MakeQArg_Addr(MqCreateTarget& createTarget, MqTranslationContexts& contexts)
{
    return visit([](auto& createTarget) -> optional<QArg_Addr> {
        using T = remove_cvref_t<decltype(createTarget)>;
        if constexpr (same_as<T, MqCreateTarget_Discard>)
            return nullopt;
        else if constexpr (same_as<T, MqCreateTarget_Slot>)
            return QArg_Addr_OfSlot{createTarget.slotIndex};
        else if constexpr (same_as<T, MqCreateTarget_Ptr>)
            return QArg_Addr_PtrSlot{createTarget.slotIndex};
        else if constexpr (same_as<T, MqCreateTarget_DirectReturn>)
            throw RuntimeFatalException{};
        else static_assert(false);
    }, createTarget);
}

// exp를 read로 읽고, CallArg로 변환한다
expected<MqEmitState<QArg_CallArg>, DiagPtr> TranslateMArgumentToQInsts_Direct(MExp* mExp, MqTranslationContexts& contexts)
{   
    auto e_s_readResult = TranslateMExpToQInstsForRead(mExp, contexts);
    RETURN_ON_ERROR_OR_DONE(e_s_readResult);

    return visit([](auto& readResult) -> QArg_CallArg {
        using T = remove_cvref_t<decltype(readResult)>;
        if constexpr (same_as<T, MqReadResult_Slot>)
            return QArg_CallArg_Slot{readResult.slotIndex};
        else if constexpr (same_as<T, MqReadResult_Ptr>)
            return QArg_CallArg_DerefPtrSlot{readResult.slotIndex};
        else if constexpr (same_as<T, MqReadResult_ConstInt32>)
            return QArg_CallArg_ConstInt32{readResult.value};
        else if constexpr (same_as<T, MqReadResult_ConstBool>)
            return QArg_CallArg_ConstBool{readResult.value};
        else static_assert(false);
    }, ** e_s_readResult);
}

expected<MqEmitState<QArg_CallArg>, DiagPtr> TranslateMArgumentToQInsts(MArgument& arg, MqParamPassingMode passingMode, MqTranslationContexts& contexts)
{
    return visit([passingMode, &contexts](auto& arg) -> expected<MqEmitState<QArg_CallArg>, DiagPtr> {
        using T = remove_cvref_t<decltype(arg)>;

        if constexpr (same_as<T, MArgument_Create>)
        {
            // passing mode가 direct 이면, exp는 read로 읽고
            if (passingMode == MqParamPassingMode::Direct)
            {
                auto& createBC = get<MCreate_BC>(arg.create);
                return TranslateMArgumentToQInsts_Direct(createBC.exp, contexts);
            }
            else if (passingMode == MqParamPassingMode::Indirect)
            {
                auto* type = GetType(arg.create, &*contexts.rFactory);
                size_t tempSlotIndex = contexts.bodyContext.AddTemp(type, "create");

                auto e_s_result = TranslateMCreateToQInsts(arg.create, MqCreateTarget_Slot{tempSlotIndex}, contexts);
                RETURN_ON_ERROR_OR_DONE(e_s_result);

                return QArg_CallArg_AddrOfSlot{tempSlotIndex};
            }
            else throw NotImplementedException{};
        }
        else if constexpr (same_as<T, MArgument_Loc>) // loc은 pointer로 넘긴다
        {
            assert(passingMode == MqParamPassingMode::Ref);

            auto e_s_locResult = TranslateMLocToQInsts(arg.loc, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_locResult);

            return MakeAddrCallArg(**e_s_locResult, contexts);
        }
        else if constexpr (same_as<T, MArgument_Move>)
        {
            assert(passingMode == MqParamPassingMode::Ref);

            // TODO: [30] move구현
            throw NotImplementedException{};
        }
        else if constexpr (same_as<T, MArgument_Forward>)
        {
            assert(passingMode == MqParamPassingMode::Forward);

            // TODO: [57] [forward] ref parameter 지원
            throw NotImplementedException{};
        }
        else if constexpr (same_as<T, MArgument_Params>) // 파라미터를 여러개 받는 경우
        {
            assert(passingMode == MqParamPassingMode::Params);
            // TODO: [31] params 구현
            throw NotImplementedException{};
        }
        else static_assert(false);
    }, arg);
}

expected<MqEmitState<void>, DiagPtr> TranslateMArgumentsToQInsts(vector<QArg_CallArg>& qArgs, vector<MArgument>& mArgs, MqFuncInfo& funcInfo, MqTranslationContexts& contexts)
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

    return MqEmitState_Ready{};
}

expected<MqEmitState<void>, DiagPtr> TranslateMInitExp_StringToQInsts(MInitExp_String* exp, MqCreateTarget createTarget, MqTranslationContexts& contexts)
{
    auto& bodyContext = contexts.bodyContext;
    assert(!exp->elements.empty());

    // 원소가 한개라면, dest에 직접 넣는다
    if (exp->elements.size() == 1)
    {
        auto e_s_result = TranslateMInitExp_StringElemToQInstsForCreate(exp->elements.front(), createTarget, contexts);
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
        auto addFuncInfo = contexts.abi->GetFuncInfo(addIntrinsicInfo, contexts.rFactory->MakeEmptyTypeArguments());

        vector<QArg_CallArg> args;
        QArg_CallArg curArg = MakeAddrCallArg(**e_s_front, contexts);
        assert(holds_alternative<MqThisPassingMode_None>(addFuncInfo.thisPassingMode));

        for (size_t i = 1, end = exp->elements.size() - 1; i < end; i++)
        {   
            auto e_s_elemLoc = TranslateMInitExp_StringElemToQInsts(exp->elements[i], contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_elemLoc);

            // 리턴값, 파라미터 넘기는 방식을 조정할 필요가 없다. 
            // string은 NBC고 NBC 리턴은 무조건 첫번째 인자로 리턴 위치를 넘기도록 되어있다

            auto elemArg = MakeAddrCallArg(**e_s_elemLoc, contexts);

            auto newSlotIndex = bodyContext.AddTemp(stringType, "stringElem_result");
            QArg_CallArg_AddrOfSlot newArg{newSlotIndex};

            bodyContext.EmitIntrinsic(
                QInst_IntrinsicKind::Add_String_StringInRef_StringInRef,
                nullopt, // string은 dest는 nullopt, 첫번째 인자로 ptr을 넣는다
                {newArg, curArg, elemArg});

            curArg = newArg;
        }
        
        auto e_s_backLoc = TranslateMInitExp_StringElemToQInsts(exp->elements.back(), contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_backLoc);
        
        if (auto o_createTargetArg = MakeAddrCallArg(createTarget, contexts))
        {
            auto backArg = MakeAddrCallArg(**e_s_backLoc, contexts);

            contexts.bodyContext.EmitIntrinsic(
                QInst_IntrinsicKind::Add_String_StringInRef_StringInRef,
                nullopt,
                {*o_createTargetArg, curArg, backArg});
        }
    }

    return MqEmitState_Ready{};
}

expected<MqEmitState<void>, DiagPtr> HandleInlineBlock(MStmt_Scope* scope, MqCreateTarget createTarget, MqTranslationContexts& contexts)
{
    // inlineBlock을 destSlot없이 호출할 일이 없도록 해야한다. 
    // 보통은 MStmt_Exp에서 호출될텐데, 여기는 Call, Assign만 가능하므로 괜찮다
    assert(holds_alternative<MqCreateTarget_Slot>(createTarget));

    // 지금 블록 말고 leaveBlock을 하나 더 만들어야 할거 같다
    auto lazyExitBlock = MakePtr<MqLazyBlock>("inline_exit");
    auto e_s_result = TranslateMStmt_ScopeToQInsts_Inline(scope, lazyExitBlock, get<MqCreateTarget_Slot>(createTarget).slotIndex, contexts);
    RETURN_ON_ERROR(e_s_result);

    // inline block이 Ready면 이상한거다
    if (*e_s_result)
        return Error<Error_InlineExp_ShouldLeaveWithValue>();

    // 내부에서 exitBlock을 쓰지 않았다면 Done이다
    if (!lazyExitBlock->HasBlock())
    {
        return MqEmitState_Done{};
    }
    else
    {
        auto* exitBlock = lazyExitBlock->GetBlock(&contexts.bodyContext);
        contexts.bodyContext.SetCurBlock(exitBlock);
        return MqEmitState_Ready{};
    }
}

void UpdateCreateTarget_Value(RType* type, size_t slotIndex, MqCreateTarget createTarget, MqTranslationContexts& contexts)
{
    visit([type, slotIndex, &contexts](auto& createTarget) {
        using T = remove_cvref_t<decltype(createTarget)>;

        if constexpr (same_as<T, MqCreateTarget_Discard>)
        {
            // do nothing
        }
        else if constexpr (same_as<T, MqCreateTarget_Slot>)
        {
            contexts.bodyContext.EmitInst(QInst_Assign{type, QArg_Dest_Slot{createTarget.slotIndex}, QArg_Value_Slot{slotIndex}});
        }
        else if constexpr (same_as<T, MqCreateTarget_Ptr>)
        {
            contexts.bodyContext.EmitInst(QInst_Store{type, QArg_Addr_PtrSlot{createTarget.slotIndex}, QArg_Value_Slot{slotIndex}});
        }
        else if constexpr (same_as<T, MqCreateTarget_DirectReturn>)
        {
            assert(type->GetCopyStrategy() == RCopyStrategy::Bitwise);
            contexts.bodyContext.EmitInst(QInst_Assign{type, QArg_Dest_DirectReturn{}, QArg_Value_Slot{slotIndex}});
        }
        else static_assert(false);
    }, createTarget);
}

void UpdateCreateTarget_Ptr(RType* type, size_t ptrSlotIndex, MqCreateTarget createTarget, MqTranslationContexts& contexts)
{
    visit([type, ptrSlotIndex, &contexts](auto& createTarget) {
        using T = remove_cvref_t<decltype(createTarget)>;

        if constexpr (same_as<T, MqCreateTarget_Discard>)
        {
            // do nothing
        }
        else if constexpr (same_as<T, MqCreateTarget_Slot>)
        {
            contexts.bodyContext.EmitInst(QInst_Load{type, QArg_Dest_Slot{createTarget.slotIndex}, QArg_Addr_PtrSlot{ptrSlotIndex}});
        }
        else if constexpr (same_as<T, MqCreateTarget_Ptr>)
        {
            size_t typeSize = contexts.abi->GetTypeSize(type);

            contexts.bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Memcpy_Void_Ptr_Ptr_Int, nullopt, {
                QArg_CallArg_Slot{createTarget.slotIndex},
                QArg_CallArg_Slot{ptrSlotIndex},
                QArg_CallArg_ConstInt32{(int)typeSize}
            });
        }
        else if constexpr (same_as<T, MqCreateTarget_DirectReturn>)
        {
            assert(type->GetCopyStrategy() == RCopyStrategy::Bitwise);
            contexts.bodyContext.EmitInst(QInst_Load{type, QArg_Dest_DirectReturn{}, QArg_Addr_PtrSlot{ptrSlotIndex}});
        }
        else static_assert(false);
   }, createTarget);
}

void UpdateCreateTarget_AddrOf(size_t slotIndex, MqCreateTarget createTarget, MqTranslationContexts& contexts)
{
    auto* type = contexts.bodyContext.GetSlotType(slotIndex);
    auto* ptrType = contexts.bodyContext.GetPtrType(type);

    visit([ptrType, slotIndex, &contexts](auto& createTarget) {
        using T = remove_cvref_t<decltype(createTarget)>;
        if constexpr (same_as<T, MqCreateTarget_Discard>)
        {
            // do nothing
        }
        else if constexpr (same_as<T, MqCreateTarget_Slot>)
        {
            contexts.bodyContext.EmitInst(QInst_AddrOf{QArg_Dest_Slot{createTarget.slotIndex}, slotIndex});
        }
        else if constexpr (same_as<T, MqCreateTarget_Ptr>)
        {
            size_t tempSlotIndex = contexts.bodyContext.AddTemp(ptrType, "addr_of");
            contexts.bodyContext.EmitInst(QInst_AddrOf{QArg_Dest_Slot{tempSlotIndex}, slotIndex});
            contexts.bodyContext.EmitInst(QInst_Store{ptrType, QArg_Addr_PtrSlot{createTarget.slotIndex}, QArg_Value_Slot{tempSlotIndex}});
        }
        else if constexpr (same_as<T, MqCreateTarget_DirectReturn>)
        {
            contexts.bodyContext.EmitInst(QInst_AddrOf{QArg_Dest_DirectReturn{}, slotIndex});
        }
        else static_assert(false);
    }, createTarget);
}

void UpdateCreateTarget(RType* type, QArg_Value&& v, MqCreateTarget createTarget, MqTranslationContexts& contexts)
{
    visit([type, &v, &contexts](auto& createTarget) {
        using T = remove_cvref_t<decltype(createTarget)>;
        if constexpr (same_as<T, MqCreateTarget_Discard>)
        {
            // do nothing
        }
        else if constexpr (same_as<T, MqCreateTarget_Slot>)
        {
            contexts.bodyContext.EmitInst(QInst_Assign{type, QArg_Dest_Slot{createTarget.slotIndex}, move(v)});
        }
        else if constexpr (same_as<T, MqCreateTarget_Ptr>)
        {
            contexts.bodyContext.EmitInst(QInst_Store{type, QArg_Addr_PtrSlot{createTarget.slotIndex}, move(v)});
        }
        else if constexpr (same_as<T, MqCreateTarget_DirectReturn>)
        {
            contexts.bodyContext.EmitInst(QInst_Assign{type, QArg_Dest_DirectReturn{}, move(v)});
        }
        else static_assert(false);
    }, createTarget);
}

expected<MqEmitState<MqLocResult>, DiagPtr> Materialize(MExp* exp, MqTranslationContexts& contexts)
{
    using ResultType = expected<MqEmitState<MqLocResult>, DiagPtr>;

    auto e_s_readResult = TranslateMExpToQInstsForRead(exp, contexts);
    RETURN_ON_ERROR_OR_DONE(e_s_readResult);

    return visit([&contexts](auto& readResult) -> ResultType {
        using T = remove_cvref_t<decltype(readResult)>;
        if constexpr (same_as<T, MqReadResult_Slot>)
        {
            return MqLocResult_Slot{readResult.slotIndex};
        }
        else if constexpr (same_as<T, MqReadResult_Ptr>)
        {
            return MqLocResult_Ptr{readResult.slotIndex};
        }
        else if constexpr (same_as<T, MqReadResult_ConstBool>)
        {
            auto* boolType = contexts.rFactory->MakeBoolType();
            size_t slotIndex = contexts.bodyContext.AddTemp(boolType, "materialize");
            contexts.bodyContext.EmitInst(QInst_Assign{boolType, QArg_Dest_Slot{slotIndex}, QArg_Value_ConstBool{readResult.value}});
            return MqLocResult_Slot{slotIndex};
        }
        else if constexpr (same_as<T, MqReadResult_ConstInt32>)
        {
            auto* intType = contexts.rFactory->MakeIntType();
            size_t slotIndex = contexts.bodyContext.AddTemp(intType, "materialize");
            contexts.bodyContext.EmitInst(QInst_Assign{intType, QArg_Dest_Slot{slotIndex}, QArg_Value_ConstInt32{readResult.value}});
            return MqLocResult_Slot{slotIndex};
        }
        else static_assert(false);
    }, **e_s_readResult);
}

expected<MqEmitState<MqLocResult>, DiagPtr> Materialize(MCreate& create, MqTranslationContexts& contexts)
{
    using ResultType = expected<MqEmitState<MqLocResult>, DiagPtr>;

    return visit([&contexts](auto& create) -> ResultType {
        using T = remove_cvref_t<decltype(create)>;
        if constexpr (same_as<T, MCreate_BC>)
        {
            return Materialize(create.exp, contexts);
        }
        else if constexpr (same_as<T, MCreate_NBC>)
        {
            RType* type = GetType(create.initExp, &*contexts.rFactory);
            size_t slotIndex = contexts.bodyContext.AddTemp(type, "materialize");
            auto e_s_result = TranslateMInitExpToQInsts(create.initExp, MqCreateTarget_Slot{slotIndex}, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_result);

            return MqLocResult_Slot{slotIndex};
        }
        else static_assert(false);
    }, create);
}


} // namespace Citron