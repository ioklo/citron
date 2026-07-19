#pragma once

#include <memory>
#include <expected>
#include <vector>
#include <optional>
#include <string>
#include <tuple>

#include "Infra/Ptr.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RFuncReturn.h"
#include "RSymbol/RFuncDecl.h"
#include "MIR/MRead.h"
#include "MIR/MScopeKind.h"
#include "BodyRes.h"
#include "FuncContext.h"

namespace Citron {

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

TranslationContexts MakeTranslationContexts(RFuncDecl* rFuncDecl, bool bSeqFunc, TakeRef<LoggerPtr> logger, TakeRef<RFactoryPtr> rFactory, TakeRef<MFactoryPtr> mFactory, TakeRef<SRTFactoryPtr> srtFactory, TakeRef<BinOpQueryServicePtr> binOpQueryService);

TranslationContexts MakeTranslationContexts_DefaultScope(TranslationContexts& contexts);
TranslationContexts MakeTranslationContexts_LoopScope(size_t o_labelId, TranslationContexts& contexts);
TranslationContexts MakeTranslationContexts_SwitchScope(std::optional<std::string>& o_label, TranslationContexts& contexts);
std::tuple<size_t, TranslationContexts> MakeTranslationContexts_InlineScope(std::optional<std::string> o_label, RType* hintType, TranslationContexts& contexts);

template<typename TFunc>
concept UsingTranslationContexts_InlineScopeFunc = requires(TFunc&& func, MScopeKind_Inline scopeKind, TranslationContexts& contexts)
{
    { func(std::move(scopeKind), contexts) }; // 리턴 타입은 체크 안함
};

template<typename TFunc> requires UsingTranslationContexts_InlineScopeFunc<TFunc>
auto UsingTranslationContexts_InlineScope(std::optional<std::string> o_label, RType* hintType, TranslationContexts& contexts, TFunc&& func)
{
    auto [labelId, newContexts] = MakeTranslationContexts_InlineScope(o_label, hintType, contexts);
    return func(MScopeKind_Inline{labelId}, newContexts);
}

TranslationContexts MakeTranslationContexts_Lambda(RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic, TranslationContexts& contexts);

std::vector<ITransactionable*> BeginTransaction(TranslationContexts& contexts);

std::expected<MInitExp_As*, DiagPtr> MakeMInitExp_As(MRead&& target, RType* testType, TranslationContexts& contexts);

std::expected<BodyRes, DiagPtr> ResolveIdentifier(InRef<RName> name, TranslationContexts& contexts);


} // namespace Citron
