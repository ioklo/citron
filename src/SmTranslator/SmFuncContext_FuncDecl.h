#pragma once
#include "SmFuncContext.h"

namespace Citron {

class RFuncDecl;

using RFactoryPtr = std::shared_ptr<class RFactory>;
using MFactoryPtr = std::shared_ptr<class MFactory>;

// FuncDecl인 경우
class SmFuncContext_FuncDecl : public SmFuncContext
{
    RFuncDecl* rFuncDecl;
    bool bSeqFunc;
    RFactoryPtr rFactory;
    MFactoryPtr mFactory;

public:
    SmFuncContext_FuncDecl(RFuncDecl* rFuncDecl, bool bSeqFunc, TakeRef<RFactoryPtr> rFactory, TakeRef<MFactoryPtr> mFactory);

public: // from SmFuncContext
    void BeginTransaction_FuncContext() override { }
    void CommitTransaction_FuncContext() override { }
    void RollbackTransaction_FuncContext() override { }

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