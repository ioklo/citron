#pragma once

#include <memory>
#include <IR0/RFuncReturn.h>

using RFuncDeclPtr = std::shared_ptr<class RFuncDecl>;

namespace Citron {

using LoggerPtr = std::shared_ptr<class Logger>;
using RTypeFactoryPtr = std::shared_ptr<class RTypeFactory>;

namespace SyntaxIR0Translator {

using GlobalContextPtr = std::shared_ptr<class GlobalContext>;
using BodyContextPtr = std::shared_ptr<class BodyContext>;
using ScopeContextPtr = std::shared_ptr<class ScopeContext>;
using BinOpQueryServicePtr = std::shared_ptr<class BinOpQueryService>;
using TranslationContextPtr = std::shared_ptr<class TranslationContext>;


class TranslationContext
{
public:
    GlobalContextPtr globalContext;
    BodyContextPtr bodyContext;
    ScopeContextPtr scopeContext;
    LoggerPtr logger;
    RTypeFactoryPtr factory;
    BinOpQueryServicePtr binOpQueryService;

public:
    // ScopeContext::MakeNewScopeContext
    static TranslationContext New(const RFuncDeclPtr& funcDecl, bool bSeqFunc, const RFuncReturn& funcReturn);

    TranslationContext MakeNestedScopeContext();
    TranslationContext MakeNestedLoopScopeContext();
    std::tuple<TranslationContext, RLambdaDecl> MakeLambdaBodyContext(RFuncReturn&& funcRet, std::vector<RFuncParameter> funcParams, bool bLastParamVariadic);

};


} // namespace SyntaxIR0Translator

} // namespace Citron