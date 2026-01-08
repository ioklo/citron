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
    auto newFuncContext = MakePtr<FuncContext_Lambda>(contexts.funcContext, contexts.scopeContext, /*bSeqFunc*/ false, move(funcRet), move(funcParams), bLastParamVariadic);
    auto newScopeContext = MakePtr<ScopeContext>(newFuncContext, nullptr, 0, contexts.rFactory);

    return {contexts.globalContext, newFuncContext, newScopeContext, contexts.logger, contexts.mFactory, contexts.rFactory, contexts.srtFactory, contexts.binOpQueryService};
}

expected<MExp*, DiagPtr> MakeMExp_As(MExp* targetExp, RType* testType, TranslationContexts& contexts)
{
    auto targetType = targetExp->GetType();
    auto targetTypeKind = targetType->GetCustomTypeKind();
    auto testTypeKind = testType->GetCustomTypeKind();

    // 5가지 케이스로 나뉜다
    if (testTypeKind == RCustomTypeKind::Class)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return contexts.mFactory->MakeMExp<MExp_ClassAsClass>(targetExp, testType, contexts.rFactory);

        else if (targetTypeKind == RCustomTypeKind::Interface)
            return contexts.mFactory->MakeMExp<MExp_InterfaceAsClass>(targetExp, testType, contexts.rFactory);
        else
            throw NotImplementedException{}; // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::Interface)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return contexts.mFactory->MakeMExp<MExp_ClassAsInterface>(targetExp, testType, contexts.rFactory);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return contexts.mFactory->MakeMExp<MExp_InterfaceAsInterface>(targetExp, testType, contexts.rFactory);
        else
            throw NotImplementedException{}; // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::EnumElem)
    {
        if (targetTypeKind == RCustomTypeKind::Enum)
            return contexts.mFactory->MakeMExp<MExp_EnumAsEnumElem>(targetExp, testType, contexts.rFactory);
        else
            throw NotImplementedException{}; // 에러 처리
    }
    else
        throw NotImplementedException{}; // 에러 처리
}

expected<ImExp*, DiagPtr> ResolveIdentifier(const RName& name, RTypeArguments* typeArgs, TranslationContexts& contexts)
{
    // struct S<T>
    // {
    //    struct X<V> { }
    //    void F<U>()
    //    {
    //        X<int> x; // 여기서 (X<>, [int])는? (S<>.X<>, [T, int]) // 컴파일 타임엔 여기까지인 거고, 실행중엔 (S<>.X<>, [T, int])는 S<>.F<>의 타입 컨텍스트(예) [string, bool]
    //    }
    // }

    auto e_o_rMember = contexts.scopeContext->ResolveIdentifier(name, typeArgs->GetCount());
    RETURN_ON_ERROR(e_o_rMember)

        if (!*e_o_rMember)
            return unexpected{MakePtr<Error_ResolveIdentifier_NotFound>()};

    return visit([&contexts, typeArgs](auto& rMember) -> ImExp* {
        using T = remove_cvref_t<decltype(rMember)>;

        if constexpr (same_as<T, RMember_LocalVar>)
        {
            return contexts.srtFactory->MakeImExp<ImExp_LocalVar>(rMember.type, rMember.name);
        }
        else if constexpr (same_as<T, RMember_LocalRef>)
        {
            return contexts.srtFactory->MakeImExp<ImExp_LocalRef>(rMember.type, rMember.name);
        }
        else if constexpr (same_as<T, RMember_GlobalFuncs>)
        {
            return contexts.srtFactory->MakeImExp<ImExp_GlobalFuncs>(rMember.items, typeArgs);
        }
        else if constexpr (same_as<T, RMember_StructVar>)
        {
            assert(typeArgs->GetCount() == 0); // ResolveIdentifier가 typeArgs가 있는데 *var를 돌려줬을리가 없다
            return contexts.srtFactory->MakeImExp<ImExp_StructVar>(rMember.decl, rMember.typeArgs, /*hasExplicitInstance*/ false, /*explicitInstance*/ nullptr);
        }
        else if constexpr (same_as<T, RMember_ClassVar>)
        {
            assert(typeArgs->GetCount() == 0);
            return contexts.srtFactory->MakeImExp<ImExp_ClassVar>(rMember.decl, rMember.typeArgs, /*hasExplicitInstance*/ false, /*explicitInstance*/ nullptr);
        }
        else if constexpr (same_as<T, RMember_Struct>)
        {
            auto* mergedTypeArgs = contexts.rFactory->MergeTypeArguments(*rMember.outerTypeArgs, *typeArgs);
            return contexts.srtFactory->MakeImExp<ImExp_Struct>(rMember.decl, mergedTypeArgs);
        }
        else
        {
            throw NotImplementedException{};
        }
    }, **e_o_rMember);
}


} // namespace Citron