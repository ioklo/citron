#pragma once
#include "FuncContext.h"

namespace Citron {

class RFuncDecl;

using RFactoryPtr = std::shared_ptr<class RFactory>;
using MFactoryPtr = std::shared_ptr<class MFactory>;

// FuncDecl인 경우
class FuncContext_FuncDecl : public FuncContext
{
    RFuncDecl* rFuncDecl;
    bool bSeqFunc;
    RFactoryPtr rFactory;
    MFactoryPtr mFactory;

public:
    FuncContext_FuncDecl(RFuncDecl* rFuncDecl, bool bSeqFunc, TakeRef<RFactoryPtr> rFactory, TakeRef<MFactoryPtr> mFactory);

public: // from FuncContext
    void BeginTransaction_FuncContext() override { }
    void CommitTransaction_FuncContext() override { }
    void RollbackTransaction_FuncContext() override { }

    bool CanAccess(RDecl* target) override;
    RTypeDecl* ResolveTypeDecl(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    std::expected<std::optional<BodyRes>, DiagPtr> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;

    RFuncReturn GetUnboundFuncReturn() override;
    void SetOpenFuncReturn(RType* retType) override;

    RTypeArguments* MakeOpenTypeArgs() override;

    bool IsSeqFunc() override;
    MLoc_This* MakeThisLoc() override;
};

} // namespace Citron