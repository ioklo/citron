#pragma once

#include <memory>
#include <vector>

namespace Citron {

class STypeExp;
class RTypeFactory;
class RDecl;
class RLoc_This;

using RTypePtr = std::shared_ptr<class RType>;
enum class SBinaryOpKind;

namespace SyntaxIR0Translator {

struct BinOpInfo;
class BinOpQueryService;
using BodyContextPtr = std::shared_ptr<class BodyContext>;

class ScopeContext
{
    std::shared_ptr<BinOpQueryService> binOpQueryService;

public:
    BodyContextPtr bodyContext;

public:
    ScopeContext(BodyContextPtr& bodyContext, const std::shared_ptr<BinOpQueryService>& binOpQueryService);

    bool IsFailed();
    RTypePtr MakeType(STypeExp& typeExp, RTypeFactory& factory);
    const std::vector<BinOpInfo>& GetBinOpInfos(SBinaryOpKind kind);

    bool IsListType(const RTypePtr& type, RTypePtr* itemType);
    bool CanAccess(RDecl& decl);

    std::shared_ptr<RLoc_This> MakeThisLoc(RTypeFactory& factory);

    std::shared_ptr<ScopeContext> MakeNestedScopeContext(std::shared_ptr<ScopeContext> sharedThis);
    std::shared_ptr<ScopeContext> MakeLoopNestedScopeContext(std::shared_ptr<ScopeContext> sharedThis);

    void AddLocalVarInfo(const RTypePtr& type, const RName& name);
    
};

using ScopeContextPtr = std::shared_ptr<ScopeContext>;

} // namespace SyntaxIR0Translator

} // namespace Citron