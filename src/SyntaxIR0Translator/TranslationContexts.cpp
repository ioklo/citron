#include "TranslationContexts.h"

#include "Infra/Expected.h"
#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RFactory.h"
#include "MIR/MExp.h"
#include "MIR/MInitExp.h"
#include "MIR/MFactory.h"
#include "ImExp.h"
#include "ScopeContext.h"
#include "SRTFactory.h"
#include "FuncContext_FuncDecl.h"
#include "FuncContext_Lambda.h"
#include "GlobalContext.h"
#include "Misc.h"

using namespace std;

namespace Citron {

TranslationContexts MakeTranslationContexts(
    NFuncDecl* nFuncDecl,
    const LoggerPtr& logger,
    const RFactoryPtr& rFactory, const MFactoryPtr& mFactory, const SRTFactoryPtr& srtFactory,
    const BinOpQueryServicePtr& binOpQueryService)
{
    auto globalContext = MakePtr<GlobalContext>();
    auto funcContext = MakePtr<FuncContext_FuncDecl>(nFuncDecl, rFactory, mFactory);
    auto scopeContext = MakePtr<ScopeContext>(funcContext, /*parentContext*/nullptr, MScopeKind_Default{}, /*curContinueLabelId*/nullopt, /*curBreakLabelId*/nullopt, rFactory);

    return {globalContext, funcContext, scopeContext, logger, mFactory, rFactory, srtFactory, binOpQueryService};
}

TranslationContexts MakeTranslationContexts_DefaultScope(TranslationContexts& contexts)
{
    // continue, break 라벨을 갱신하지 않고, 상위 scope의 라벨을 그대로 사용한다
    auto newScopeContext = MakePtr<ScopeContext>(contexts.funcContext, contexts.scopeContext, MScopeKind_Default{}, contexts.scopeContext->GetCurContinueLabelId(), contexts.scopeContext->GetCurBreakLabelId(), contexts.rFactory);
    return {contexts.globalContext, contexts.funcContext, newScopeContext, contexts.logger, contexts.mFactory, contexts.rFactory, contexts.srtFactory, contexts.binOpQueryService};
}

TranslationContexts MakeTranslationContexts_LoopScope(size_t labelId, TranslationContexts& contexts)
{
    auto newScopeContext = MakePtr<ScopeContext>(contexts.funcContext, contexts.scopeContext, MScopeKind_Loop{labelId}, labelId, labelId, contexts.rFactory);
    return {contexts.globalContext, contexts.funcContext, newScopeContext, contexts.logger, contexts.mFactory, contexts.rFactory, contexts.srtFactory, contexts.binOpQueryService};
}

TranslationContexts MakeTranslationContexts_SwitchScope(optional<string> o_label, TranslationContexts& contexts)
{
    size_t labelId = contexts.funcContext->AddNewLabelId(o_label);
    // switch는 break만 갱신한다
    auto newScopeContext = MakePtr<ScopeContext>(contexts.funcContext, contexts.scopeContext, MScopeKind_Switch{labelId}, contexts.scopeContext->GetCurContinueLabelId(), labelId, contexts.rFactory);
    return {contexts.globalContext, contexts.funcContext, newScopeContext, contexts.logger, contexts.mFactory, contexts.rFactory, contexts.srtFactory, contexts.binOpQueryService};
}

TranslationContexts MakeTranslationContexts_Lambda(RFuncReturn&& funcRet, vector<RFuncParameter>&& funcParams, bool bLastParamVariadic, TranslationContexts& contexts)
{
    auto newFuncContext = MakePtr<FuncContext_Lambda>(contexts.funcContext, contexts.scopeContext, /*bSeqFunc*/false, move(funcRet), move(funcParams), bLastParamVariadic);
    auto newScopeContext = MakePtr<ScopeContext>(newFuncContext, /*parentContext*/nullptr, MScopeKind_Default{}, /*curContinueLabelId*/nullopt, /*curBreakLabelId*/nullopt, contexts.rFactory);

    return {contexts.globalContext, newFuncContext, newScopeContext, contexts.logger, contexts.mFactory, contexts.rFactory, contexts.srtFactory, contexts.binOpQueryService};
}


expected<MInitExp_As*, DiagPtr> MakeMInitExp_As(MRead&& target, RType* testType, TranslationContexts& contexts)
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

expected<BodyRes, DiagPtr> ResolveIdentifier(const RName& name, size_t memberTypeArgsCount, TranslationContexts& contexts)
{
    // struct S<T>
    // {
    //    struct X<V> { }
    //    void F<U>()
    //    {
    //        X<int> x; // 여기서 (X<>, [int])는? (S<>.X<>, [T, int]) // 컴파일 타임엔 여기까지인 거고, 실행중엔 (S<>.X<>, [T, int])는 S<>.F<>의 타입 컨텍스트(예) [string, bool]
    //    }
    // }

    auto e_o_bodyRes = contexts.scopeContext->ResolveIdentifier(name, memberTypeArgsCount);
    RETURN_ON_ERROR(e_o_bodyRes);

    if (!*e_o_bodyRes)
        return unexpected{MakePtr<Error_ResolveIdentifier_NotFound>()};

    return move(**e_o_bodyRes);
}


} // namespace Citron