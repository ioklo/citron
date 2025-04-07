export module Citron.SyntaxIR0Translator:ScopeContext;

import <memory>;
import <vector>;
import <optional>;
import <string>;
import <unordered_map>;
import <expected>;

import Citron.Diag;
import Citron.Syntax;
import Citron.RDecls;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class ScopeContext;
export using ScopeContextPtr = std::shared_ptr<ScopeContext>;

export class FuncContext;
export using FuncContextPtr = std::shared_ptr<FuncContext>;

export class ImExp;
export using ImExpPtr = std::shared_ptr<ImExp>;

export class CloneContext;
export class UpdateContext;

export class ScopeContext
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
    std::expected<RTypePtr, DiagPtr> TranslateSTypeExpToRType(STypeExp& typeExp, RTypeFactory& factory);

    std::shared_ptr<NLoc_This> MakeThisLoc(RTypeFactory& factory);
    std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory);
};

export using ScopeContextPtr = std::shared_ptr<ScopeContext>;

} // namespace Citron::SyntaxIR0Translator