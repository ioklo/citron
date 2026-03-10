#include "TranslationContexts.h"

#include "Infra/Expected.h"
#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"

#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RFactory.h"

#include "MIR/MExp.h"
#include "MIR/MFactory.h"

#include "ImExp.h"

#include "ScopeContext.h"
#include "SRTFactory.h"
#include "FuncContext_FuncDecl.h"
#include "FuncContext_Lambda.h"
#include "GlobalContext.h"

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
    auto scopeContext = MakePtr<ScopeContext>(funcContext, nullptr, 0, rFactory);

    return {globalContext, funcContext, scopeContext, logger, mFactory, rFactory, srtFactory, binOpQueryService};
}


TranslationContexts MakeTranslationContexts_NestedScope(TranslationContexts& contexts)
{
    auto newScopeContext = MakePtr<ScopeContext>(contexts.funcContext, contexts.scopeContext, contexts.scopeContext->GetNestedLoopCount(), contexts.rFactory);
    return {contexts.globalContext, contexts.funcContext, newScopeContext, contexts.logger, contexts.mFactory, contexts.rFactory, contexts.srtFactory, contexts.binOpQueryService};
}

TranslationContexts MakeTranslationContexts_NestedLoop(TranslationContexts& contexts)
{
    auto newScopeContext = MakePtr<ScopeContext>(contexts.funcContext, contexts.scopeContext, contexts.scopeContext->GetNestedLoopCount() + 1, contexts.rFactory);
    return {contexts.globalContext, contexts.funcContext, newScopeContext, contexts.logger, contexts.mFactory, contexts.rFactory, contexts.srtFactory, contexts.binOpQueryService};
}

TranslationContexts MakeTranslationContexts_Lambda(RFuncReturn&& funcRet, vector<RFuncParameter>&& funcParams, bool bLastParamVariadic, TranslationContexts& contexts)
{
    auto newFuncContext = MakePtr<FuncContext_Lambda>(contexts.funcContext, contexts.scopeContext, /*bSeqFunc*/false, move(funcRet), move(funcParams), bLastParamVariadic);
    auto newScopeContext = MakePtr<ScopeContext>(newFuncContext, nullptr, 0, contexts.rFactory);

    return {contexts.globalContext, newFuncContext, newScopeContext, contexts.logger, contexts.mFactory, contexts.rFactory, contexts.srtFactory, contexts.binOpQueryService};
}

expected<MExp*, DiagPtr> MakeMExp_As(MExp* targetExp, RType* testType, TranslationContexts& contexts)
{
    auto targetType = targetExp->GetType();
    auto targetTypeKind = targetType->GetTypeKind();
    auto testTypeKind = testType->GetTypeKind();

    // 5가지 케이스로 나뉜다
    if (testTypeKind == RTypeKind::Class)
    {
        if (targetTypeKind == RTypeKind::Class)
            return contexts.mFactory->MakeMExp<MExp_ClassAsClass>(targetExp, testType, contexts.rFactory);

        else if (targetTypeKind == RTypeKind::Interface)
            return contexts.mFactory->MakeMExp<MExp_InterfaceAsClass>(targetExp, testType, contexts.rFactory);
        else
            throw NotImplementedException{}; // 에러 처리
    }
    else if (testTypeKind == RTypeKind::Interface)
    {
        if (targetTypeKind == RTypeKind::Class)
            return contexts.mFactory->MakeMExp<MExp_ClassAsInterface>(targetExp, testType, contexts.rFactory);
        else if (targetTypeKind == RTypeKind::Interface)
            return contexts.mFactory->MakeMExp<MExp_InterfaceAsInterface>(targetExp, testType, contexts.rFactory);
        else
            throw NotImplementedException{}; // 에러 처리
    }
    else if (auto* enumElemTestType = dynamic_cast<RType_EnumElem*>(testType))
    {
        if (dynamic_cast<RType_Enum*>(targetType))
            return contexts.mFactory->MakeMExp<MExp_EnumAsEnumElem>(targetExp, enumElemTestType, contexts.rFactory);
        else
            throw NotImplementedException{}; // 에러 처리
    }
    else
        throw NotImplementedException{}; // 에러 처리
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