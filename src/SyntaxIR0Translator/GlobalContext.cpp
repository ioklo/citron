#include "pch.h"
#include "GlobalContext.h"

#include <cassert>

#include <Infra/Ptr.h>

#include "BinOpQueryService.h"
#include "BodyContext.h"

namespace Citron::SyntaxIR0Translator {

GlobalContext::GlobalContext(const ModuleDeclsPtr& moduleDecls, const LoggerPtr& logger, const RTypeFactoryPtr& factory, const std::shared_ptr<BinOpQueryService>& binOpQueryService)
    : moduleDecls(moduleDecls), binOpQueryService(binOpQueryService)
{
}

GlobalContextPtr GlobalContext::Clone(CloneContext& cloneContext)
{
    auto clonedLogger = cloneContext.GetClone(this->logger);
    return MakePtr<GlobalContext>(moduleDecls, clonedLogger, factory, binOpQueryService);
}

void GlobalContext::Update(const GlobalContextPtr& src, UpdateContext& updateContext)
{
    assert(moduleDecls == src->moduleDecls);
    context.Update(logger, src->logger);
    assert(factory == src->factory);
    assert(binOpQueryService == src->binOpQueryService);
}

const std::vector<BinOpInfo>& GlobalContext::GetBinOpInfos(SBinaryOpKind kind)
{
    return binOpQueryService->GetInfos(kind);
}

ScopeContextPtr GlobalContext::MakeNewScopeContext(const GlobalContextPtr& sharedThis, const RFuncDeclPtr& funcDecl, const RTypeArgumentsPtr& typeArgs, bool bSeqFunc, const RFuncReturn& funcReturn)
{
    auto newBodyContext = MakePtr<BodyContext>(moduleDecls, /*outerScopeContext*/ nullptr, funcDecl, typeArgs, bSeqFunc, funcReturn);
    return MakePtr<ScopeContext>(sharedThis, newBodyContext, /*parentContext*/ nullptr, /*bLoop*/ false);
}

} // namespace Citron::SyntaxIR0Translator