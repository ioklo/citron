#pragma once

#include <memory>
#include <expected>
#include <vector>

#include <RSymbol/RNames.h>
#include <RSymbol/RFuncReturn.h>

namespace Citron {

class NFuncDecl;
class ImExp;
class MExp;
class RTypeArguments;
class ITransactionable;
struct RFuncParameter;

using GlobalContextPtr = std::shared_ptr<class GlobalContext>;
using FuncContextPtr = std::shared_ptr<class FuncContext>;
using ScopeContextPtr = std::shared_ptr<class ScopeContext>;
using BinOpQueryServicePtr = std::shared_ptr<class BinOpQueryService>;
using MFactoryPtr = std::shared_ptr<class MFactory>;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using LoggerPtr = std::shared_ptr<class Logger>;
using SRTFactoryPtr = std::shared_ptr<class SRTFactory>;

using DiagPtr = std::shared_ptr<struct Diag>;

struct TranslationContexts
{
    GlobalContextPtr globalContext;
    FuncContextPtr funcContext;
    ScopeContextPtr scopeContext;
    LoggerPtr logger;
    MFactoryPtr mFactory;
    RFactoryPtr rFactory;
    SRTFactoryPtr srtFactory;
    BinOpQueryServicePtr binOpQueryService;
};

TranslationContexts MakeTranslationContexts(NFuncDecl* nFuncDecl, const LoggerPtr& logger, const RFactoryPtr& rFactory, const MFactoryPtr& mFactory, const SRTFactoryPtr& srtFactory, const BinOpQueryServicePtr& binOpQueryService);

TranslationContexts MakeTranslationContexts_NestedScope(TranslationContexts& contexts);
TranslationContexts MakeTranslationContexts_NestedLoop(TranslationContexts& contexts);
TranslationContexts MakeTranslationContexts_Lambda(RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic, TranslationContexts& contexts);

std::vector<ITransactionable*> BeginTransaction(TranslationContexts& contexts);

std::expected<MExp*, DiagPtr> MakeMExp_As(MExp* targetExp, RType* testType, TranslationContexts& contexts);

std::expected<ImExp*, DiagPtr> ResolveIdentifier(const RName& name, RTypeArguments* typeArgs, TranslationContexts& contexts);

} // namespace Citron
