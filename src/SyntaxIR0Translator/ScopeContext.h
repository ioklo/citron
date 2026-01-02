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
#include "RSymbol/RMember.h"
#include "RSymbol/RFuncReturn.h"

namespace Citron { 

struct RFuncParameter;
class RType;
class RFactory;

class MLoc_This;
class NLambdaDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;

using ScopeContextPtr = std::shared_ptr<class ScopeContext>;
using FuncContextPtr = std::shared_ptr<class FuncContext>;
class ImExp;

class CloneContext;
class UpdateContext;

class ScopeContext
{
public:
    FuncContextPtr funcContext;
    ScopeContextPtr parentContext;
    int nestedLoop;

    RFactoryPtr rFactory;

    // 로컬 관리
    std::unordered_map<RName, RType*> locals;

public:
    ScopeContext(const FuncContextPtr& funcContext, const ScopeContextPtr& parentContext, int nestedLoop, const RFactoryPtr& rFactory);

    ScopeContextPtr Clone(CloneContext& context);
    void Update(ScopeContext& src, UpdateContext& context);

public:
    RTypeArguments* MakeOpenTypeArgs();
    void SetFlowEndsCompletely();

    std::shared_ptr<ScopeContext> MakeTranslationContexts_NestedScope(std::shared_ptr<ScopeContext> sharedThis);
    std::shared_ptr<ScopeContext> MakeLoopNestedScopeContext(std::shared_ptr<ScopeContext> sharedThis);
    std::tuple<ScopeContextPtr, NLambdaDecl> MakeTranslationContexts_Lambda(const RFuncReturn& ret, std::vector<RFuncParameter> params, bool bLastParamVariadic);

    void AddLocalVarInfo(RType* type, const RName& name);
    // std::optional<LocalVarInfo> GetLocalVarInfo(const RName& name);

    bool DoesLocalVarNameExistInScope(const RName& name);

    bool IsFailed();
    bool IsInLoop() { return nestedLoop != 0; }
    std::expected<RType*, DiagPtr> TranslateSTypeExpToRType(STypeExp* sTypeExp);
    std::expected<std::optional<RMember>, DiagPtr> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount);
};

using ScopeContextPtr = std::shared_ptr<ScopeContext>;

} // namespace Citron