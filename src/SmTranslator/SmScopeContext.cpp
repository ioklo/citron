#include "SmScopeContext.h"

#include <ranges>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"

#include "Syntax/Syntax.h"
#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RDeclRes.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RFuncParameter.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RLambdaDecl.h"
#include "RSymbol/RTypeRes.h"
#include "RSymbol/RClassDecl.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/REnumDecl.h"
#include "RSymbol/REnumElemDecl.h"
#include "RSymbol/RTraitDecl.h"

#include "SmFuncContext.h"

using namespace std;

namespace Citron {

SmScopeContext::SmScopeContext(
    const SmFuncContextPtr& funcContext, 
    const SmScopeContextPtr& parentContext, 
    MScopeKind&& scopeKind, 
    optional<size_t> curContinueLabelId, 
    optional<size_t> curBreakLabelId, 
    shared_ptr<InlineScopeContext> inlineScopeContext,
    TakeRef<RFactoryPtr> rFactory)
    : funcContext{funcContext}
    , parentContext{parentContext}
    , scopeKind{std::move(scopeKind)}
    , curContinueLabelId{curContinueLabelId}
    , curBreakLabelId{curBreakLabelId}
    , inlineScopeContext{move(inlineScopeContext)}
    , rFactory{rFactory.Take()}
{
}

void SmScopeContext::BeginTransaction()
{
    transactionInfos.emplace_back();

    if (parentContext)
        return parentContext->BeginTransaction();
    
    funcContext->BeginTransaction();
}

void SmScopeContext::CommitTransaction()
{
    auto& transactionInfo = transactionInfos.back();

    if (2 <= transactionInfos.size())
    {
        auto& parentTransactionInfo = transactionInfos[transactionInfos.size() - 2];
        parentTransactionInfo.deltaLocalInfos.merge(transactionInfo.deltaLocalInfos);
        assert(transactionInfo.deltaLocalInfos.empty());
    }
    else
    {
        localInfos.merge(transactionInfo.deltaLocalInfos);
        assert(transactionInfo.deltaLocalInfos.empty());
    }

    transactionInfos.pop_back();

    if (parentContext)
        return parentContext->CommitTransaction();

    return funcContext->CommitTransaction();
}

void SmScopeContext::RollbackTransaction()
{
    transactionInfos.pop_back();

    if (parentContext)
        return parentContext->RollbackTransaction();

    return funcContext->RollbackTransaction();
}

void SmScopeContext::SetFlowEndsCompletely()
{
    throw NotImplementedException{};
}

void SmScopeContext::AddLocalVarInfo(RType* type, InRef<RName> name)
{
    if (transactionInfos.empty())
    {
        auto [i, b] = localInfos.try_emplace(*name, LocalInfo{LocalInfoKind::Var, type});
        assert(b);
    }
    else
    {
        assert(!DoesLocalNameExistInScope(name));
        transactionInfos.back().deltaLocalInfos.emplace(*name, LocalInfo{LocalInfoKind::Var, type});
    }
}

void SmScopeContext::AddLocalRefInfo(RType* type, InRef<RName> name)
{
    if (transactionInfos.empty())
    {
        auto [i, b] = localInfos.try_emplace(*name, LocalInfo{LocalInfoKind::Ref, type});
        assert(b);
    }
    else
    {
        assert(!DoesLocalNameExistInScope(name));
        transactionInfos.back().deltaLocalInfos.emplace(*name, LocalInfo{LocalInfoKind::Ref, type});
    }
}

bool SmScopeContext::DoesLocalNameExistInScope(InRef<RName> name)
{
    if (!transactionInfos.empty())
    {
        for (auto& transactionInfo : transactionInfos | views::reverse)
        {
            auto i = transactionInfo.deltaLocalInfos.find(*name);
            if (i != transactionInfo.deltaLocalInfos.end())
                return true;
        }
    }

    auto i = localInfos.find(*name);
    return i != localInfos.end();
}

bool SmScopeContext::IsFailed() 
{
    throw NotImplementedException{};
}

std::optional<MScopeKind> SmScopeContext::GetReachableScopeKind(size_t labelId)
{
    return visit([this, labelId](auto& scopeKind) -> optional<MScopeKind> {
        using T = remove_cvref_t<decltype(scopeKind)>;
        if constexpr (same_as<T, MScopeKind_Default>)
        {
        }   
        else if constexpr (same_as<T, MScopeKind_Loop>)
        {
            if (scopeKind.labelId == labelId) return scopeKind;
        }
        else if constexpr (same_as<T, MScopeKind_Switch>)
        {
            if (scopeKind.labelId == labelId) return scopeKind;
        }
        else if constexpr (same_as<T, MScopeKind_Inline>)
        {
            if (scopeKind.labelId == labelId) return scopeKind;
        }
        else
            static_assert(false);

        return parentContext ? parentContext->GetReachableScopeKind(labelId) : nullopt;
    }, scopeKind);
}

expected<RType*, DiagPtr> MakeType(RTypeRes& typeRes, RTypeArguments* memberTypeArgs, RFactory* rFactory)
{
    return typeRes.Visit([memberTypeArgs, rFactory](auto& typeRes) -> expected<RType*, DiagPtr> {
        using T = remove_cvref_t<decltype(typeRes)>;

        if constexpr (same_as<T, RTypeRes_Namespace>) 
        {
            return Error<Error_ResolveIdentifier_CantUseNamespaceAsType>();
        }
        else if constexpr (same_as<T, RTypeRes_Class>) 
        {
            // 타입 인자 검사
            if (typeRes.decl->GetTypeParamCount() != memberTypeArgs->GetCount())
                return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();

            auto* typeArgs = rFactory->MergeTypeArguments(typeRes.outerTypeArgs, memberTypeArgs);
            return rFactory->MakeClassType(typeRes.decl, typeArgs);
        }
        else if constexpr (same_as<T, RTypeRes_Struct>) 
        {
            // 타입 인자 검사
            if (typeRes.decl->GetTypeParamCount() != memberTypeArgs->GetCount())
                return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();

            auto* typeArgs = rFactory->MergeTypeArguments(typeRes.outerTypeArgs, memberTypeArgs);
            return rFactory->MakeStructType(typeRes.decl, typeArgs);
        }
        else if constexpr (same_as<T, RTypeRes_Enum>) 
        {
            // 타입 인자 검사
            if (typeRes.decl->GetTypeParamCount() != memberTypeArgs->GetCount())
                return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();

            auto* typeArgs = rFactory->MergeTypeArguments(typeRes.outerTypeArgs, memberTypeArgs);
            return rFactory->MakeEnumType(typeRes.decl, typeArgs);
        }
        else if constexpr (same_as<T, RTypeRes_EnumElem>) 
        {
            // 타입 인자 검사
            if (typeRes.decl->GetTypeParamCount() != memberTypeArgs->GetCount())
                return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();

            auto* typeArgs = rFactory->MergeTypeArguments(typeRes.outerTypeArgs, memberTypeArgs);
            return rFactory->MakeEnumElemType(typeRes.decl, typeArgs);
        }
        else if constexpr (same_as<T, RTypeRes_Interface>) 
        {
            // TODO: [71] 2026-07-18, interface 구현
            throw NotImplementedException{};
        }
        else if constexpr (same_as<T, RTypeRes_Lambda>) 
        {
            // TODO: [65] 2026-07-06, RLambdaDecl제거, RStructDecl을 쓰도록 변경
            throw NotImplementedException{};
        }
        else if constexpr (same_as<T, RTypeRes_TypeVar>) 
        {
            return rFactory->MakeTypeVarType(typeRes.decl);
        }
        else if constexpr (same_as<T, RTypeRes_Trait>) 
        {
            return Error<Error_ResolveIdentifier_CantUseTraitAsType>();
        }
        else static_assert(false);
    });
}

expected<RType*, DiagPtr> SmScopeContext::TranslateSTypeExpToRType(STypeExp* sTypeExp)
{
    // TODO: BuildNonTypeSymbolContext::MakeType 에도 같은 코드가 있다
    struct Visitor
    {
        using ResultType = expected<RType*, DiagPtr>;

        RFactory* rFactory;
        SmScopeContext& scopeContext;

        expected<RType*, DiagPtr> Visit(STypeExp_Id* idExp)
        {
            // 예약어 처리
            if (idExp->name == "void" && idExp->typeArgs.empty())
                return rFactory->MakeVoidType();
            else if (idExp->name == "bool" && idExp->typeArgs.empty())
                return rFactory->MakeBoolType();
            else if (idExp->name == "int" && idExp->typeArgs.empty())
                return rFactory->MakeIntType();
            else if (idExp->name == "string" && idExp->typeArgs.empty())
                return rFactory->MakeStringType();

            auto o_rTypeRes = scopeContext.funcContext->ResolveTypeIdentifier(RName::Normal(idExp->name));
            if (!o_rTypeRes) return nullptr;

            vector<RType*> memberTypeArgsVector;
            memberTypeArgsVector.reserve(idExp->typeArgs.size());
            for (auto* sTypeArg : idExp->typeArgs)
            {
                auto e_rTypeArg = scopeContext.TranslateSTypeExpToRType(sTypeArg);
                if (!e_rTypeArg) return nullptr;
                memberTypeArgsVector.push_back(*e_rTypeArg);
            }
            auto* memberTypeArgs = rFactory->MakeTypeArguments(memberTypeArgsVector);
            return MakeType(*o_rTypeRes, memberTypeArgs, rFactory);
        }

        expected<RType*, DiagPtr> Visit(STypeExp* e)
        {
            throw NotImplementedException{};
        }

    } visitor{rFactory.get(), *this};

    return Accept(visitor, sTypeExp);
}

expected<optional<BodyRes>, DiagPtr> SmScopeContext::ResolveIdentifier(InRef<RName> name)
{
    // 로컬을 검색한다
    if (!transactionInfos.empty())
    {
        for (auto& transactionInfo : transactionInfos | views::reverse)
        {
            auto i = transactionInfo.deltaLocalInfos.find(*name);
            if (i != transactionInfo.deltaLocalInfos.end())
            {
                if (i->second.kind == LocalInfoKind::Var)
                    return BodyRes_LocalVar{i->second.type, *name};
                else if (i->second.kind == LocalInfoKind::Ref)
                    return BodyRes_LocalRef(i->second.type, *name);
                else assert(false);
            }
        }
    }
    
    auto i = localInfos.find(*name);
    if (i != localInfos.end())
    {
        if (i->second.kind == LocalInfoKind::Var)
            return BodyRes_LocalVar{i->second.type, *name};
        else if (i->second.kind == LocalInfoKind::Ref)
            return BodyRes_LocalRef{i->second.type, *name};
        else assert(false);
    }

    // 상위 스코프가 있으면 그곳을 검색한다
    if (parentContext)
        return parentContext->ResolveIdentifier(name);

    // 상위 스코프가 없으면 scope가 속해있는 함수 컨텍스트를 검색한다
    return funcContext->ResolveIdentifier(name);
}

};