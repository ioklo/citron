export module Citron.SyntaxIR0Translator:TranslationContext;

import <memory>;
import <vector>;
import <optional>;
import <string>;
import <expected>;

import Citron.Diag;
import Citron.Syntax;
import Citron.RDecls;
import Citron.NDecls;

import :DesignatedDiagnostic;
import :DeclTypeInfo;
import :ResolveIdentifierError;

namespace Citron::SyntaxIR0Translator {

export class ReExp;
export class IrExp_BoxRef;
export struct BinOpInfo;

export class ImExp;
export using ImExpPtr = std::shared_ptr<ImExp>;

export class GlobalContext;
export using GlobalContextPtr = std::shared_ptr<GlobalContext>;

export class FuncContext;
export using FuncContextPtr = std::shared_ptr<FuncContext>;

export class ScopeContext;
export using ScopeContextPtr = std::shared_ptr<ScopeContext>;

export class BinOpQueryService;
export using BinOpQueryServicePtr = std::shared_ptr<BinOpQueryService>;

export class TranslationContext;
export using TranslationContextPtr = std::shared_ptr<TranslationContext>;

export struct NLambdaDeclAndArgs
{
    std::shared_ptr<NLambdaDecl> decl;
    std::vector<NArgument> args;   // ctor args
};

export class TranslationContext
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

    void SetSyntax(const SSyntaxPtr& syntax);

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

    std::expected<ImExpPtr, ResolveIdentifierError> ResolveIdentifier(RName&& name, RTypeArgumentsPtr&& typeArgs);

public: // for BinOpQueryService
    const std::vector<BinOpInfo>& GetBinOpInfos(SBinaryOpKind kind);
};

} // namespace Citron::SyntaxIR0Translator