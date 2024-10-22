#pragma once

#include <memory>
#include <vector>
#include <IR0/RFuncReturn.h>
#include "DesignatedErrorLogger.h"

using RFuncDeclPtr = std::shared_ptr<class RFuncDecl>;

namespace Citron {

class STypeExp;
class RExp;
class RLoc;
class RLoc_This;

enum class SBinaryOpKind;

using LoggerPtr = std::shared_ptr<class Logger>;
using RTypeFactoryPtr = std::shared_ptr<class RTypeFactory>;
using RTypePtr = std::shared_ptr<class RType>;
using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;

namespace SyntaxIR0Translator {

class ReExp;
class IrExp_BoxRef;
struct BinOpInfo;

using GlobalContextPtr = std::shared_ptr<class GlobalContext>;
using BodyContextPtr = std::shared_ptr<class BodyContext>;
using ScopeContextPtr = std::shared_ptr<class ScopeContext>;
using BinOpQueryServicePtr = std::shared_ptr<class BinOpQueryService>;
using TranslationContextPtr = std::shared_ptr<class TranslationContext>;


class TranslationContext
{
    GlobalContextPtr globalContext;
    BodyContextPtr bodyContext;
    ScopeContextPtr scopeContext;
    LoggerPtr logger;
    RTypeFactoryPtr factory;
    BinOpQueryServicePtr binOpQueryService;

    TranslationContext(const GlobalContextPtr& globalContext, const BodyContextPtr& bodyContext, const ScopeContextPtr& scopeContext, const LoggerPtr& logger, const RTypeFactoryPtr& factory, const BinOpQueryServicePtr& binOpQueryService);

public:
    // ScopeContext::MakeNewScopeContext
    static TranslationContext New(const RFuncDeclPtr& funcDecl, bool bSeqFunc, const RFuncReturn& funcReturn);

    TranslationContext MakeNestedScopeContext();
    TranslationContext MakeNestedLoopScopeContext();
    TranslationContext MakeLambdaBodyContext(RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic);

    DesignatedErrorLogger MakeDesignatedErrorLogger(void (Logger::* func)());

    std::shared_ptr<RLoc_This> MakeThisLoc();
    RExpPtr MakeRExp_As(RExpPtr&& targetExp, const RTypePtr& testType);

public: // for scopeContext
    bool IsInLoop();

public: // for bodyContext
    bool CanAccess(RDecl* target);
    bool IsSeqFunc();
    RFuncReturn GetFuncReturn();
    void SetFuncReturn(RTypePtr&& retType);

public: // for logging
    template<typename TFunc>
    void Log(TFunc func)
    {
        (logger->*func)();
    }

    void SetSyntax(const SSyntaxPtr& syntax);

public:
    RTypePtr TranslateSTypeExpToRType(STypeExp& typeExp);

public: // for type factory
    RTypePtr GetType(RLoc& loc);
    RTypePtr GetType(ReExp& reExp);
    RTypePtr GetType(RExp& exp);

    RTypePtr GetTargetType(IrExp_BoxRef& boxRef);

    RTypeArgumentsPtr MakeTypeArguments(const std::vector<RTypePtr>& items);
    RTypeArgumentsPtr MergeTypeArguments(RTypeArguments& typeArgs0, RTypeArguments& typeArgs1);

    RTypePtr MakeVoidType();
    RTypePtr MakeBoolType();
    RTypePtr MakeIntType();
    RTypePtr MakeStringType();

    bool IsListType(const RTypePtr& type, RTypePtr* outItemType);

public: // for BinOpQueryService
    const std::vector<BinOpInfo>& GetBinOpInfos(SBinaryOpKind kind);
};


} // namespace SyntaxIR0Translator

} // namespace Citron