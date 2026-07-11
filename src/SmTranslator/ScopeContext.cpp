#include "ScopeContext.h"

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

#include "FuncContext.h"

using namespace std;

namespace Citron {

ScopeContext::ScopeContext(
    const FuncContextPtr& funcContext, 
    const ScopeContextPtr& parentContext, 
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

void ScopeContext::BeginTransaction()
{
    transactionInfos.emplace_back();

    if (parentContext)
        return parentContext->BeginTransaction();
    
    funcContext->BeginTransaction();
}

void ScopeContext::CommitTransaction()
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

void ScopeContext::RollbackTransaction()
{
    transactionInfos.pop_back();

    if (parentContext)
        return parentContext->RollbackTransaction();

    return funcContext->RollbackTransaction();
}

void ScopeContext::SetFlowEndsCompletely()
{
    throw NotImplementedException{};
}

void ScopeContext::AddLocalVarInfo(RType* type, InRef<RName> name)
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

void ScopeContext::AddLocalRefInfo(RType* type, InRef<RName> name)
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

bool ScopeContext::DoesLocalNameExistInScope(InRef<RName> name)
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

bool ScopeContext::IsFailed() 
{
    throw NotImplementedException{};
}

std::optional<MScopeKind> ScopeContext::GetReachableScopeKind(size_t labelId)
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

expected<RType*, DiagPtr> ScopeContext::TranslateSTypeExpToRType(STypeExp* sTypeExp)
{
    // TODO: BuildTypeDependentSymbolContext::MakeType 에도 같은 코드가 있다
    struct Visitor
    {
        using ResultType = RType*;

        RFactory* rFactory;
        ScopeContext& scopeContext;

        RType* Visit(STypeExp_Id* idExp)
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

            auto* rTypeDecl = scopeContext.funcContext->ResolveTypeDecl(RName::Normal(idExp->name), idExp->typeArgs.size());
            if (!rTypeDecl) return nullptr;

            vector<RType*> rTypeArgVector;
            rTypeArgVector.reserve(idExp->typeArgs.size());
            for (auto* sTypeArg : idExp->typeArgs)
            {
                auto e_rTypeArg = scopeContext.TranslateSTypeExpToRType(sTypeArg);
                if (!e_rTypeArg) return nullptr;
                rTypeArgVector.push_back(*e_rTypeArg);
            }
            auto* rTypeArgs = rFactory->MakeTypeArguments(rTypeArgVector);
            return rFactory->MakeType(rTypeDecl, rTypeArgs);
        }

        RType* Visit(STypeExp* e)
        {
            throw NotImplementedException{};
        }
    } visitor{rFactory.get(), *this};

    return Accept(visitor, sTypeExp);
}

expected<optional<BodyRes>, DiagPtr> ScopeContext::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
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
        return parentContext->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);

    // 상위 스코프가 없으면 scope가 속해있는 함수 컨텍스트를 검색한다
    return funcContext->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

};