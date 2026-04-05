#pragma once

#include <memory>
#include <expected>
#include <vector>
#include <optional>
#include <string>

#include "Infra/Ptr.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RFuncReturn.h"
#include "MIR/MRead.h"
#include "MIR/MScopeKind.h"
#include "BodyRes.h"
#include "FuncContext.h"

namespace Citron {

class NFuncDecl;
struct ImExp;
struct MExp;
struct MInitExp_As;
class RTypeArguments;
class ITransactionable;
struct RFuncParameter;

struct MStmt_Scope;

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

TranslationContexts MakeTranslationContexts_DefaultScope(TranslationContexts& contexts);
TranslationContexts MakeTranslationContexts_LoopScope(size_t o_labelId, TranslationContexts& contexts);
TranslationContexts MakeTranslationContexts_SwitchScope(std::optional<std::string> o_label, TranslationContexts& contexts);
TranslationContexts MakeTranslationContexts_Lambda(RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic, TranslationContexts& contexts);

std::vector<ITransactionable*> BeginTransaction(TranslationContexts& contexts);

std::expected<MInitExp_As*, DiagPtr> MakeMInitExp_As(MRead&& target, RType* testType, TranslationContexts& contexts);

std::expected<BodyRes, DiagPtr> ResolveIdentifier(const RName& name, size_t memberTypeArgs, TranslationContexts& contexts);


} // namespace Citron
