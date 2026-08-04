#include "SmScopeContext.h"

#include <ranges>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"

#include "Syntax/Syntax.h"
#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RFuncParameter.h"
#include "RSymbol/RFactory.h"

#include "SmFuncContext.h"
#include "SmTypeRes.h"
#include "SmTypeTranslation.h"
#include "SmTypeResolveScope.h"
#include "SmTypeTranslationContexts.h"

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

expected<RType*, DiagPtr> SmScopeContext::TranslateSTypeExpToRType(STypeExp* sTypeExp)
{
    SmTypeTranslationContexts contexts{SmTypeResolveScope_FuncContext{funcContext.get()}, rFactory.get()};
    return Citron::TranslateSTypeExpToRType(sTypeExp, contexts);
}

expected<optional<SmBodyRes>, DiagPtr> SmScopeContext::ResolveIdentifier(InRef<RName> name)
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
                    return SmBodyRes_LocalVar{i->second.type, *name};
                else if (i->second.kind == LocalInfoKind::Ref)
                    return SmBodyRes_LocalRef(i->second.type, *name);
                else assert(false);
            }
        }
    }
    
    auto i = localInfos.find(*name);
    if (i != localInfos.end())
    {
        if (i->second.kind == LocalInfoKind::Var)
            return SmBodyRes_LocalVar{i->second.type, *name};
        else if (i->second.kind == LocalInfoKind::Ref)
            return SmBodyRes_LocalRef{i->second.type, *name};
        else assert(false);
    }

    // 상위 스코프가 있으면 그곳을 검색한다
    if (parentContext)
        return parentContext->ResolveIdentifier(name);

    // 상위 스코프가 없으면 scope가 속해있는 함수 컨텍스트를 검색한다
    return funcContext->ResolveIdentifier(name);
}

};