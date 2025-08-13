#pragma once

#include <memory>
#include <vector>
#include <optional>
#include <string>
#include <unordered_map>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "IR0/RNames.h"
#include "IR0/RMember.h"
#include "IR0/RFuncReturn.h"

namespace Citron { 

struct RFuncParameter;
class RType;
class IR0Factory;

class NLoc_This;
class NLambdaDecl;

namespace SyntaxIR0Translator {

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

class FuncContext;
using FuncContextPtr = std::shared_ptr<FuncContext>;

class ImExp;

class CloneContext;
class UpdateContext;

class ScopeContext
{
public:
    FuncContextPtr funcContext;
    ScopeContextPtr parentContext;
    int nestedLoop;

    // 로컬 관리
    std::unordered_map<std::string, RType*> locals;

public:
    ScopeContext(const FuncContextPtr& funcContext, const ScopeContextPtr& parentContext, int nestedLoop);

    ScopeContextPtr Clone(CloneContext& context);
    void Update(ScopeContext& src, UpdateContext& context);

public:
    RTypeArguments* MakeOpenTypeArgs(IR0Factory& factory);
    void SetFlowEndsCompletely();

    std::shared_ptr<ScopeContext> MakeNestedScopeContext(std::shared_ptr<ScopeContext> sharedThis);
    std::shared_ptr<ScopeContext> MakeLoopNestedScopeContext(std::shared_ptr<ScopeContext> sharedThis);
    std::tuple<ScopeContextPtr, NLambdaDecl> MakeLambdaBodyContext(const RFuncReturn& ret, std::vector<RFuncParameter> params, bool bLastParamVariadic);

    void AddLocalVarInfo(RType* type, const RName& name);
    // std::optional<LocalVarInfo> GetLocalVarInfo(const RName& name);

    bool DoesLocalVarNameExistInScope(const std::string& name);

    bool IsFailed();
    bool IsInLoop() { return nestedLoop != 0; }
    std::expected<RType*, DiagPtr> TranslateSTypeExpToRType(STypeExp& typeExp, IR0Factory& factory);

    std::shared_ptr<NLoc_This> MakeThisLoc(IR0Factory& factory);
    std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, IR0Factory& factory);
};

using ScopeContextPtr = std::shared_ptr<ScopeContext>;

} // namespace SyntaxIR0Translator
} // namespace Citron