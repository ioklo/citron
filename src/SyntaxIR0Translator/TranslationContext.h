#pragma once

#include <memory>
#include <vector>
#include <optional>
#include <string>
#include <expected>
#include <concepts>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RFuncReturn.h"
#include "RSymbol/RNames.h"
#include "MIR/MFactory.h"
#include "MIR/MArgument.h"

#include "SRTFactory.h"
#include "DesignatedDiagnostic.h"

namespace Citron {

struct RFuncParameter;

class RFuncDecl;
class RDecl;
class RType_Enum;
class RType_EnumElem;
class RTypeArguments;

class MLoc;
class MStmt;
class NLambdaDecl;
class MLoc_This;
class NFuncDecl;

using MFactoryPtr = std::shared_ptr<class MFactory>;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using LoggerPtr = std::shared_ptr<class Logger>;

class ReExp;
class IrExp_BoxRef;
struct BinOpInfo;

class ImExp;
class IrExp;

using GlobalContextPtr = std::shared_ptr<class GlobalContext>;
using FuncContextPtr = std::shared_ptr<class FuncContext>;
using ScopeContextPtr = std::shared_ptr<class ScopeContext>;
using BinOpQueryServicePtr = std::shared_ptr<class BinOpQueryService>;

struct NLambdaDeclAndArgs
{
    NLambdaDecl* decl;
    std::vector<MArgument> args;   // ctor args
};

class TranslationContext
{
    GlobalContextPtr globalContext;
    FuncContextPtr funcContext;
    ScopeContextPtr scopeContext;
    LoggerPtr logger;
    MFactoryPtr mFactory;
    RFactoryPtr rFactory;
    SRTFactoryPtr srtFactory;
    BinOpQueryServicePtr binOpQueryService;

    TranslationContext(
        const GlobalContextPtr& globalContext, const FuncContextPtr& funcContext, const ScopeContextPtr& scopeContext, 
        const LoggerPtr& logger, const RFactoryPtr& rfactory, const MFactoryPtr& mFactory, const SRTFactoryPtr& srtFactory,
        const BinOpQueryServicePtr& binOpQueryService);

public:
    // ScopeContext::MakeNewScopeContext
    static TranslationContext Make(
        NFuncDecl* nFuncDecl, 
        const LoggerPtr& logger,
        const RFactoryPtr& rFactory, const MFactoryPtr& mFactory, const SRTFactoryPtr& srtFactory,
        const BinOpQueryServicePtr& binOpQueryService);

    TranslationContext MakeNestedScopeContext();
    TranslationContext MakeNestedLoopScopeContext();
    TranslationContext MakeLambdaBodyContext(RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic);

    MLoc_This* MakeThisLoc();
    std::expected<MExp*, DiagPtr> MakeMExp_As(MExp* targetExp, RType* testType);

public: // for scopeContext
    ScopeContext& GetScopeContext() { return *scopeContext; }

public: // for funcContext
    bool CanAccess(RDecl* target);
    bool IsSeqFunc();
    RFuncReturn GetUnboundFuncReturn();
    void SetOpenFuncReturn(RType* retType);
    NLambdaDeclAndArgs MakeLambdaDeclAndArgs(std::vector<MStmt*>&& body);

public: // for logging
    template<typename TFunc>
    void Log(TFunc func)
    {
        (logger.get()->*func)();
    }

public:
    std::expected<RType*, DiagPtr> TranslateSTypeExpToRType(STypeExp* typeExp);

public: // for type rFactory
    RType* GetType(MLoc* loc);
    RType* GetType(ReExp* reExp);
    RType* GetType(MExp* exp);

    RType* GetTargetType(IrExp_BoxRef* boxRef);

    RTypeArguments* MakeTypeArguments(const std::vector<RType*>& items);
    RTypeArguments* MergeTypeArguments(RTypeArguments& typeArgs0, RTypeArguments& typeArgs1);

    RType* MakeVoidType();
    RType* MakeBoolType();
    RType* MakeIntType();
    RType* MakeStringType();

    bool IsListType(RType* type, RType** outItemType);

    RFuncReturn GetFuncReturn(RFuncDecl& decl, RTypeArguments& typeArgs);
    RFuncParameter GetFuncParam(RFuncDecl& decl, RTypeArguments& typeArgs, size_t index);

    RType_Enum* GetBaseEnumType(RType_EnumElem& enumElemType);

    std::expected<ImExp*, DiagPtr> ResolveIdentifier(const RName& name, RTypeArguments* typeArgs);

    template<typename TMStmt, typename... TArgs> requires std::derived_from<TMStmt, MStmt>
    TMStmt* MakeNStmt(TArgs&&... args)
    {
        return mFactory->MakeMStmt<TMStmt>(std::forward<TArgs>(args)...);
    }

    template<typename TMExp, typename... TArgs> requires std::derived_from<TMExp, MExp>
    TMExp* MakeMExp(TArgs&&... args)
    {
        return mFactory->MakeMExp<TMExp>(std::forward<TArgs>(args)...);
    }

    template<typename TMLoc, typename... TArgs> requires std::derived_from<TMLoc, MLoc>
    TMLoc* MakeNLoc(TArgs&&... args)
    {
        return mFactory->MakeMLoc<TMLoc>(std::forward<TArgs>(args)...);
    }

    template<typename TImExp, typename... TArgs> requires std::derived_from<TImExp, ImExp>
    TImExp* MakeImExp(TArgs&&... args)
    {
        return srtFactory->MakeImExp<TImExp>(std::forward<TArgs>(args)...);
    }

    template<typename TIrExp, typename... TArgs> requires std::derived_from<TIrExp, IrExp>
    TIrExp* MakeIrExp(TArgs&&... args)
    {
        return srtFactory->MakeIrExp<TIrExp>(std::forward<TArgs>(args)...);
    }

    template<typename TReExp, typename... TArgs> requires std::derived_from<TReExp, ReExp>
    TReExp* MakeReExp(TArgs&&... args)
    {
        return srtFactory->MakeReExp<TReExp>(std::forward<TArgs>(args)...);
    }

public: // for BinOpQueryService
    const std::vector<BinOpInfo>& GetBinOpInfos(SBinaryOpKind kind);
};

} // namespace Citron
