#pragma once

#include <memory>
#include <vector>
#include <optional>
#include <string>
#include <unordered_map>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RFuncReturn.h"
#include "MIR/MScopeKind.h"
#include "BodyRes.h"

namespace Citron { 

struct RFuncParameter;
class RType;
class RFactory;

struct MLoc_This;
class NLambdaDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;

using ScopeContextPtr = std::shared_ptr<class ScopeContext>;
using FuncContextPtr = std::shared_ptr<class FuncContext>;
struct ImExp;

class CloneContext;
class UpdateContext;

class ScopeContext
{
private: // transaction에 영향 받지 않는 변수들 (인자로 들어온)
    FuncContextPtr funcContext;
    ScopeContextPtr parentContext;

    MScopeKind scopeKind;
    // cached
    std::optional<size_t> curContinueLabelId;
    std::optional<size_t> curBreakLabelId;

private: // dependency
    RFactoryPtr rFactory;

private: // transaction에 영향 받는 변수들
    // 로컬 관리, var, ref
    enum class LocalInfoKind { Var, Ref };
    struct LocalInfo
    {
        LocalInfoKind kind;
        RType* type;
    };

    std::unordered_map<RName, LocalInfo> localInfos;

private: // for transaction
    struct TransactionInfo
    {   
        // localInfo는 delta만 더 저장한다
        std::unordered_map<RName, LocalInfo> deltaLocalInfos;
    };

    std::vector<TransactionInfo> transactionInfos;

public:
    ScopeContext(const FuncContextPtr& funcContext, const ScopeContextPtr& parentContext, MScopeKind&& scopeKind, std::optional<size_t> curContinueLabelId, std::optional<size_t> curBreakLabelId, const RFactoryPtr& rFactory);

    void BeginTransaction();
    void CommitTransaction();
    void RollbackTransaction();

public:
    void SetFlowEndsCompletely();

    void AddLocalVarInfo(RType* type, const RName& name);
    void AddLocalRefInfo(RType* type, const RName& name);
    // std::optional<LocalVarInfo> GetLocalVarInfo(const RName& name);

    bool DoesLocalNameExistInScope(const RName& name);

    bool IsFailed();
    std::optional<size_t> GetCurContinueLabelId() { return curContinueLabelId; }
    std::optional<size_t> GetCurBreakLabelId() { return curBreakLabelId; }
    std::optional<MScopeKind> GetReachableScopeKind(size_t o_labelId);

    std::expected<RType*, DiagPtr> TranslateSTypeExpToRType(STypeExp* sTypeExp);
    std::expected<std::optional<BodyRes>, DiagPtr> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount);
};

using ScopeContextPtr = std::shared_ptr<ScopeContext>;

} // namespace Citron