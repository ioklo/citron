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
#include "SmFuncContext.h"

namespace Citron {

struct ImExp;
struct MExp;
struct MInitExp_As;
class RTypeArguments;
class ITransactionable;
struct RFuncParameter;

struct MStmt_Scope;

using SmGlobalContextPtr = std::shared_ptr<class SmGlobalContext>;
using SmFuncContextPtr = std::shared_ptr<class SmFuncContext>;
using SmScopeContextPtr = std::shared_ptr<class SmScopeContext>;
using BinOpQueryServicePtr = std::shared_ptr<class BinOpQueryService>;
using MFactoryPtr = std::shared_ptr<class MFactory>;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using LoggerPtr = std::shared_ptr<class Logger>;
using SRTFactoryPtr = std::shared_ptr<class SRTFactory>;

using DiagPtr = std::shared_ptr<struct Diag>;

struct SmTranslationContexts
{
    SmGlobalContextPtr globalContext;
    SmFuncContextPtr funcContext;
    SmScopeContextPtr scopeContext;
    LoggerPtr logger;
    MFactoryPtr mFactory;
    RFactoryPtr rFactory;
    SRTFactoryPtr srtFactory;
    BinOpQueryServicePtr binOpQueryService;
};

SmTranslationContexts MakeTranslationContexts(RFuncDecl* rFuncDecl, bool bSeqFunc, TakeRef<LoggerPtr> logger, TakeRef<RFactoryPtr> rFactory, TakeRef<MFactoryPtr> mFactory, TakeRef<SRTFactoryPtr> srtFactory, TakeRef<BinOpQueryServicePtr> binOpQueryService);

SmTranslationContexts MakeTranslationContexts_DefaultScope(SmTranslationContexts& contexts);
SmTranslationContexts MakeTranslationContexts_LoopScope(size_t o_labelId, SmTranslationContexts& contexts);
SmTranslationContexts MakeTranslationContexts_SwitchScope(std::optional<std::string>& o_label, SmTranslationContexts& contexts);
std::tuple<size_t, SmTranslationContexts> MakeTranslationContexts_InlineScope(std::optional<std::string> o_label, RType* hintType, SmTranslationContexts& contexts);

template<typename TFunc>
concept UsingTranslationContexts_InlineScopeFunc = requires(TFunc&& func, MScopeKind_Inline scopeKind, SmTranslationContexts& contexts)
{
    { func(std::move(scopeKind), contexts) }; // 리턴 타입은 체크 안함
};

template<typename TFunc> requires UsingTranslationContexts_InlineScopeFunc<TFunc>
auto UsingTranslationContexts_InlineScope(std::optional<std::string> o_label, RType* hintType, SmTranslationContexts& contexts, TFunc&& func)
{
    auto [labelId, newContexts] = MakeTranslationContexts_InlineScope(o_label, hintType, contexts);
    return func(MScopeKind_Inline{labelId}, newContexts);
}

SmTranslationContexts MakeTranslationContexts_Lambda(RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic, SmTranslationContexts& contexts);

std::vector<ITransactionable*> BeginTransaction(SmTranslationContexts& contexts);

std::expected<MInitExp_As*, DiagPtr> MakeMInitExp_As(MRead&& target, RType* testType, SmTranslationContexts& contexts);

std::expected<BodyRes, DiagPtr> ResolveIdentifier(InRef<RName> name, SmTranslationContexts& contexts);


} // namespace Citron
