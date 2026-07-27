#pragma once
#include "SmFuncContext.h"

namespace Citron {

struct RFuncParameter;
using MFactoryPtr = std::shared_ptr<class MFactory>;
using SmFuncContextPtr = std::shared_ptr<class SmFuncContext>;
using SmScopeContextPtr = std::shared_ptr<class SmScopeContext>;

// 람다인 경우
class SmFuncContext_Lambda : public SmFuncContext
{
    SmFuncContextPtr outerFunc;
    SmScopeContextPtr outerScope;
    bool bSeqFunc; // reserved
    RFuncReturn funcReturn;
    std::vector<RFuncParameter> funcParams;
    bool bLastParamVariadic;

    MFactoryPtr mFactory;

public:
    SmFuncContext_Lambda(TakeRef<SmFuncContextPtr> outerFunc, TakeRef<SmScopeContextPtr> outerScope, bool bSeqFunc, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic);

public: // from SmFuncContext
    void BeginTransaction_FuncContext() override {}
    void CommitTransaction_FuncContext() override {}
    void RollbackTransaction_FuncContext() override {}

    bool CanAccess(RDecl* target) override;
    std::optional<SmTypeRes> ResolveTypeIdentifier(InRef<RName> name) override;
    std::expected<std::optional<SmBodyRes>, DiagPtr> ResolveIdentifier(InRef<RName> name) override;

    RFuncReturn GetUnboundFuncReturn() override;
    void SetOpenFuncReturn(RType* retType) override;

    RTypeArguments* MakeOpenTypeArgs() override;

    bool IsSeqFunc() override;
    MLoc_This* MakeThisLoc() override;
};

} // namespace Citron