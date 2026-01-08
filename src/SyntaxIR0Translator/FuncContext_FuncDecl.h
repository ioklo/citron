#pragma once
#include "FuncContext.h"

namespace Citron {

using RFactoryPtr = std::shared_ptr<class RFactory>;
using MFactoryPtr = std::shared_ptr<class MFactory>;
class NFuncDecl;

// FuncDecl인 경우
class FuncContext_FuncDecl : public FuncContext
{
    NFuncDecl* nFuncDecl;
    RFactoryPtr rFactory;
    MFactoryPtr mFactory;

public:
    FuncContext_FuncDecl(NFuncDecl* funcDecl, const RFactoryPtr& rFactory, const MFactoryPtr& mFactory);

public: // from FuncContext
    void BeginTransaction_FuncContext() override { }
    void CommitTransaction_FuncContext() override { }
    void RollbackTransaction_FuncContext() override { }

    bool CanAccess(RDecl* target) override;
    RTypeDecl* ResolveTypeDecl(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    std::expected<std::optional<RMember>, DiagPtr> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    RFuncReturn GetUnboundFuncReturn() override;
    void SetOpenFuncReturn(RType* retType) override;

    RTypeArguments* MakeOpenTypeArgs() override;

    bool IsSeqFunc() override;
    MLoc_This* MakeThisLoc() override;
};

} // namespace Citron