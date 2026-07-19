#pragma once
#include "FuncContext.h"

namespace Citron {

struct RFuncParameter;
using MFactoryPtr = std::shared_ptr<class MFactory>;
using FuncContextPtr = std::shared_ptr<class FuncContext>;
using ScopeContextPtr = std::shared_ptr<class ScopeContext>;

// 람다인 경우
class FuncContext_Lambda : public FuncContext
{
    FuncContextPtr outerFunc;
    ScopeContextPtr outerScope;
    bool bSeqFunc; // reserved
    RFuncReturn funcReturn;
    std::vector<RFuncParameter> funcParams;
    bool bLastParamVariadic;

    MFactoryPtr mFactory;

public:
    FuncContext_Lambda(TakeRef<FuncContextPtr> outerFunc, TakeRef<ScopeContextPtr> outerScope, bool bSeqFunc, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic);

public: // from FuncContext
    void BeginTransaction_FuncContext() override {}
    void CommitTransaction_FuncContext() override {}
    void RollbackTransaction_FuncContext() override {}

    bool CanAccess(RDecl* target) override;
    RTypeDecl* ResolveTypeDecl(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    std::expected<std::optional<BodyRes>, DiagPtr> ResolveIdentifier(InRef<RName> name) override;

    RFuncReturn GetUnboundFuncReturn() override;
    void SetOpenFuncReturn(RType* retType) override;

    RTypeArguments* MakeOpenTypeArgs() override;

    bool IsSeqFunc() override;
    MLoc_This* MakeThisLoc() override;
};

} // namespace Citron