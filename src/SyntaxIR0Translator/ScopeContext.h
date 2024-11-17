#pragma once

#include <memory>
#include <vector>
#include <optional>
#include <unordered_map>

#include <IR0/RFuncReturn.h>
#include <IR0/RFuncParameter.h>
#include <IR0/RNames.h>
#include <IR0/RMember.h>

namespace Citron {

class STypeExp;
class RTypeFactory;
class NDecl;
class NLoc_This;

using RTypePtr = std::shared_ptr<class RType>;
enum class SBinaryOpKind;
class NLambdaDecl;

namespace SyntaxIR0Translator {

using ScopeContextPtr = std::shared_ptr<class ScopeContext>;
using FuncContextPtr = std::shared_ptr<class FuncContext>;
using ImExpPtr = std::shared_ptr<class ImExp>;
using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;

class CloneContext;
class UpdateContext;

class ScopeContext
{
public:
    FuncContextPtr funcContext;
    ScopeContextPtr parentContext;
    int nestedLoop;

    // 로컬 관리
    std::unordered_map<std::string, RTypePtr> locals;

public:
    ScopeContext(const FuncContextPtr& funcContext, const ScopeContextPtr& parentContext, int nestedLoop);

    ScopeContextPtr Clone(CloneContext& context);
    void Update(ScopeContext& src, UpdateContext& context);

public:
    RTypeArgumentsPtr MakeOpenTypeArgs(RTypeFactory& factory);
    void SetFlowEndsCompletely();

    std::shared_ptr<ScopeContext> MakeNestedScopeContext(std::shared_ptr<ScopeContext> sharedThis);
    std::shared_ptr<ScopeContext> MakeLoopNestedScopeContext(std::shared_ptr<ScopeContext> sharedThis);
    std::tuple<ScopeContextPtr, NLambdaDecl> MakeLambdaBodyContext(const RFuncReturn& ret, std::vector<RFuncParameter> params, bool bLastParamVariadic);

    void AddLocalVarInfo(const RTypePtr& type, const RName& name);
    // std::optional<LocalVarInfo> GetLocalVarInfo(const RName& name);

    bool DoesLocalVarNameExistInScope(const std::string& name);

    bool IsFailed();
    bool IsInLoop() { return nestedLoop != 0; }
    RTypePtr TranslateSTypeExpToRType(STypeExp& typeExp, RTypeFactory& factory);

    std::shared_ptr<NLoc_This> MakeThisLoc(RTypeFactory& factory);
    std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory);
};

using ScopeContextPtr = std::shared_ptr<ScopeContext>;

} // namespace SyntaxIR0Translator

} // namespace Citron