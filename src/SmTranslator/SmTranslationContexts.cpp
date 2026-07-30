#include "SmTranslationContexts.h"

#include "Infra/Expected.h"
#include "Infra/Ptr.h"
#include "Infra/Ref.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RFactory.h"
#include "MIR/MExp.h"
#include "MIR/MInitExp.h"
#include "MIR/MFactory.h"
#include "ImExp.h"
#include "SmScopeContext.h"
#include "SmFactory.h"
#include "SmFuncContext_FuncDecl.h"
#include "SmFuncContext_Lambda.h"
#include "SmTypeTranslation.h"
#include "SmGlobalContext.h"
#include "Misc.h"

using namespace std;

namespace Citron {

expected<RTypeArguments*, DiagPtr> SmTranslationContexts::MakeRTypeArgs(std::span<STypeExp*> sTypeArgs)
{
    return Citron::MakeRTypeArgs(sTypeArgs, SmTypeResolveScope_FuncContext{funcContext.get()}, rFactory.get());
}

SmTranslationContexts MakeTranslationContexts(
    TakeRef<SmDeclContextPtr> declContext,
    RFuncDecl* rFuncDecl,
    bool bSeqFunc,
    TakeRef<LoggerPtr> logger,
    TakeRef<RFactoryPtr> rFactory, TakeRef<MFactoryPtr> mFactory, TakeRef<SmFactoryPtr> smFactory,
    TakeRef<BinOpQueryServicePtr> binOpQueryService)
{
    auto globalContext = MakePtr<SmGlobalContext>();
    auto funcContext = MakePtr<SmFuncContext_FuncDecl>(declContext.Take(), rFuncDecl, bSeqFunc, *rFactory, *mFactory);
    auto scopeContext = MakePtr<SmScopeContext>(funcContext, /*parentContext*/nullptr, MScopeKind_Default{}, /*curContinueLabelId*/nullopt, /*curBreakLabelId*/nullopt, /*inlineScopeContext*/nullptr, *rFactory);

    // scopeContext에 함수 인자를 넣는다
    auto* openTypeArgs = rFuncDecl->RFuncDecl_GetDecl()->MakeOpenTypeArgs(**rFactory);
    for (auto& funcParam : rFuncDecl->GetUnboundFuncParams())
    {
        if (funcParam.IsRef())
        {
            auto* paramType = funcParam.type->Apply(openTypeArgs);
            scopeContext->AddLocalRefInfo(paramType, funcParam.name);
        }
        else
        {
            auto* paramType = funcParam.type->Apply(openTypeArgs);
            scopeContext->AddLocalVarInfo(paramType, funcParam.name);
        }
    }

    return {globalContext, funcContext, scopeContext, logger.Take(), mFactory.Take(), rFactory.Take(), smFactory.Take(), binOpQueryService.Take()};
}

SmTranslationContexts MakeTranslationContexts_DefaultScope(SmTranslationContexts& contexts)
{
    // continue, break 라벨을 갱신하지 않고, 상위 scope의 라벨을 그대로 사용한다
    auto newScopeContext = MakePtr<SmScopeContext>(contexts.funcContext, contexts.scopeContext, MScopeKind_Default{}, 
        contexts.scopeContext->GetCurContinueLabelId(), contexts.scopeContext->GetCurBreakLabelId(), 
        contexts.scopeContext->GetInlineScopeContext(), contexts.rFactory);

    return {contexts.globalContext, contexts.funcContext, newScopeContext, contexts.logger, contexts.mFactory, contexts.rFactory, contexts.smFactory, contexts.binOpQueryService};
}

SmTranslationContexts MakeTranslationContexts_LoopScope(size_t labelId, SmTranslationContexts& contexts)
{
    auto newScopeContext = MakePtr<SmScopeContext>(contexts.funcContext, contexts.scopeContext, MScopeKind_Loop{labelId}, labelId, labelId, contexts.scopeContext->GetInlineScopeContext(), contexts.rFactory);
    return {contexts.globalContext, contexts.funcContext, newScopeContext, contexts.logger, contexts.mFactory, contexts.rFactory, contexts.smFactory, contexts.binOpQueryService};
}

SmTranslationContexts MakeTranslationContexts_SwitchScope(optional<string>& o_label, SmTranslationContexts& contexts)
{
    size_t labelId = contexts.funcContext->AddNewLabelId(o_label);
    // switch는 break만 갱신한다
    auto newScopeContext = MakePtr<SmScopeContext>(contexts.funcContext, contexts.scopeContext, MScopeKind_Switch{labelId}, contexts.scopeContext->GetCurContinueLabelId(), labelId, contexts.scopeContext->GetInlineScopeContext(), contexts.rFactory);
    return {contexts.globalContext, contexts.funcContext, newScopeContext, contexts.logger, contexts.mFactory, contexts.rFactory, contexts.smFactory, contexts.binOpQueryService};
}

tuple<size_t, SmTranslationContexts> MakeTranslationContexts_InlineScope(std::optional<std::string> o_label, RType* hintType, SmTranslationContexts& contexts)
{
    size_t labelId = contexts.funcContext->AddNewLabelId(o_label);

    auto newScopeContext = MakePtr<SmScopeContext>(
        contexts.funcContext,
        contexts.scopeContext,
        MScopeKind_Inline{labelId},
        contexts.scopeContext->GetCurContinueLabelId(),
        contexts.scopeContext->GetCurBreakLabelId(),
        MakePtr<InlineScopeContext>(labelId, hintType),
        contexts.rFactory);

    return {labelId, {contexts.globalContext, contexts.funcContext, newScopeContext, contexts.logger, contexts.mFactory, contexts.rFactory, contexts.smFactory, contexts.binOpQueryService}};
}

SmTranslationContexts MakeTranslationContexts_Lambda(RFuncReturn&& funcRet, vector<RFuncParameter>&& funcParams, bool bLastParamVariadic, SmTranslationContexts& contexts)
{
    auto newFuncContext = MakePtr<SmFuncContext_Lambda>(contexts.funcContext, contexts.scopeContext, /*bSeqFunc*/false, move(funcRet), move(funcParams), bLastParamVariadic);
    auto newScopeContext = MakePtr<SmScopeContext>(newFuncContext, /*parentContext*/nullptr, MScopeKind_Default{}, /*curContinueLabelId*/nullopt, /*curBreakLabelId*/nullopt, /*inlineScopeContext*/nullptr, contexts.rFactory);

    return {contexts.globalContext, newFuncContext, newScopeContext, contexts.logger, contexts.mFactory, contexts.rFactory, contexts.smFactory, contexts.binOpQueryService};
}


expected<MInitExp_As*, DiagPtr> MakeMInitExp_As(MRead&& target, RType* testType, SmTranslationContexts& contexts)
{
    auto* targetType = GetType(target, &*contexts.rFactory);
    auto o_kind = [targetType, testType]() -> optional<MInitExp_AsKind> {
        auto targetTypeKind = targetType->GetTypeKind();
        auto testTypeKind = testType->GetTypeKind();

        if (testTypeKind == RTypeKind::Class)
        {
            if (targetTypeKind == RTypeKind::Class) return MInitExp_AsKind::Class_Class;
            else if (targetTypeKind == RTypeKind::Interface) return MInitExp_AsKind::Interface_Class;                
        }
        else if (testTypeKind == RTypeKind::Interface)
        {
            if (targetTypeKind == RTypeKind::Class) return MInitExp_AsKind::Class_Interface;
            else if (targetTypeKind == RTypeKind::Interface) return MInitExp_AsKind::Interface_Interface;
        }
        return nullopt;
    }();

    if (!o_kind) return Error<Error_As_NotSupported>();

    return contexts.mFactory->MakeMInitExp<MInitExp_As>(MInitExp_AsKind::Class_Class, std::move(target), testType);
}

expected<SmBodyRes, DiagPtr> ResolveIdentifier(InRef<RName> name, SmTranslationContexts& contexts)
{
    // struct S<T>
    // {
    //    struct X<V> { }
    //    void F<U>()
    //    {
    //        X<int> x; // 여기서 (X<>, [int])는? (S<>.X<>, [T, int]) // 컴파일 타임엔 여기까지인 거고, 실행중엔 (S<>.X<>, [T, int])는 S<>.F<>의 타입 컨텍스트(예) [string, bool]
    //    }
    // }

    auto e_o_bodyRes = contexts.scopeContext->ResolveIdentifier(name);
    RETURN_ON_ERROR(e_o_bodyRes);

    if (!*e_o_bodyRes)
        return unexpected{MakePtr<Error_ResolveIdentifier_NotFound>()};

    return move(**e_o_bodyRes);
}

} // namespace Citron