#pragma once

#include <memory>
#include <vector>
#include <optional>
#include <string>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "IR0/RFuncReturn.h"
#include "IR0/RNames.h"
#include "IR0/NArgument.h"

#include "DesignatedDiagnostic.h"
#include "DeclTypeInfo.h"
#include "ResolveIdentifierError.h"

namespace Citron {

struct RFuncParameter;
class RTypeFactory;
using RTypeFactoryPtr = std::shared_ptr<RTypeFactory>;
class RFuncDecl;
using RFuncDeclPtr = std::shared_ptr<RFuncDecl>;
class RDecl;
class RType_Enum;
class RType_EnumElem;
class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

class NLoc;
class NStmt;
using NStmtPtr = std::shared_ptr<NStmt>;
class NLambdaDecl;
class NLoc_This;

namespace SyntaxIR0Translator {

class ReExp;
class IrExp_BoxRef;
struct BinOpInfo;

class ImExp;
using ImExpPtr = std::shared_ptr<ImExp>;

class GlobalContext;
using GlobalContextPtr = std::shared_ptr<GlobalContext>;

class FuncContext;
using FuncContextPtr = std::shared_ptr<FuncContext>;

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

class BinOpQueryService;
using BinOpQueryServicePtr = std::shared_ptr<BinOpQueryService>;

class TranslationContext;
using TranslationContextPtr = std::shared_ptr<TranslationContext>;

struct NLambdaDeclAndArgs
{
    std::shared_ptr<NLambdaDecl> decl;
    std::vector<NArgument> args;   // ctor args
};

class TranslationContext
{
    GlobalContextPtr globalContext;
    FuncContextPtr funcContext;
    ScopeContextPtr scopeContext;
    LoggerPtr logger;
    RTypeFactoryPtr factory;
    BinOpQueryServicePtr binOpQueryService;

    TranslationContext(const GlobalContextPtr& globalContext, const FuncContextPtr& funcContext, const ScopeContextPtr& scopeContext, const LoggerPtr& logger, const RTypeFactoryPtr& factory, const BinOpQueryServicePtr& binOpQueryService);

public:
    // ScopeContext::MakeNewScopeContext
    static TranslationContext New(const RFuncDeclPtr& funcDecl, bool bSeqFunc, const RFuncReturn& funcReturn);

    TranslationContext MakeNestedScopeContext();
    TranslationContext MakeNestedLoopScopeContext();
    TranslationContext MakeLambdaBodyContext(RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic);

    std::shared_ptr<NLoc_This> MakeThisLoc();
    std::expected<NExpPtr, DiagPtr> MakeNExp_As(NExpPtr&& targetExp, const RTypePtr& testType);

public: // for scopeContext
    bool IsInLoop();
    DeclTypeInfo GetDeclTypeInfo(STypeExp& typeExp);
    bool DoesLocalVarNameExistInScope(const std::string& name);
    void AddLocalVarInfo(const RTypePtr& type, RName&& name);

public: // for funcContext
    bool CanAccess(RDecl* target);
    bool IsSeqFunc();
    RFuncReturn GetUnboundFuncReturn();
    void SetOpenFuncReturn(RTypePtr&& retType);
    NLambdaDeclAndArgs MakeLambdaDeclAndArgs(std::vector<NStmtPtr>&& body);

public: // for logging
    template<typename TFunc>
    void Log(TFunc func)
    {
        (logger.get()->*func)();
    }

public:
    std::expected<RTypePtr, DiagPtr> TranslateSTypeExpToRType(STypeExp& typeExp);

public: // for type factory
    RTypePtr GetType(NLoc& loc);
    RTypePtr GetType(ReExp& reExp);
    RTypePtr GetType(NExp& exp);

    RTypePtr GetTargetType(IrExp_BoxRef& boxRef);

    RTypeArgumentsPtr MakeTypeArguments(const std::vector<RTypePtr>& items);
    RTypeArgumentsPtr MergeTypeArguments(RTypeArguments& typeArgs0, RTypeArguments& typeArgs1);

    RTypePtr MakeVoidType();
    RTypePtr MakeBoolType();
    RTypePtr MakeIntType();
    RTypePtr MakeStringType();

    bool IsListType(const RTypePtr& type, RTypePtr* outItemType);

    RFuncReturn GetFuncReturn(RFuncDecl& decl, RTypeArguments& typeArgs);
    RFuncParameter GetFuncParam(RFuncDecl& decl, RTypeArguments& typeArgs, size_t index);

    std::shared_ptr<RType_Enum> GetBaseEnumType(RType_EnumElem& enumElemType);

    std::expected<ImExpPtr, std::shared_ptr<ResolveIdentifierError>> ResolveIdentifier(RName&& name, RTypeArgumentsPtr&& typeArgs);

public: // for BinOpQueryService
    const std::vector<BinOpInfo>& GetBinOpInfos(SBinaryOpKind kind);
};

} // namespace SyntaxIR0Translator
} // namespace Citron
