using System.Diagnostics.CodeAnalysis;
using Citron.Collections;
using Citron.Symbol;
using S = Citron.Syntax;
using R = Citron.IR0;

namespace Citron.Analysis;

// GlobalContext redirection
partial class ScopeContext
{   
    public void AddFatalError(SyntaxAnalysisErrorCode code, S.ISyntaxNode node) => globalContext.AddFatalError(code, node);
    public void AddError(SyntaxAnalysisErrorCode code, S.ISyntaxNode node) => globalContext.AddError(code, node);

    public IType GetVoidType() => globalContext.GetVoidType();
    public IType GetBoolType() => globalContext.GetBoolType();
    public IType GetIntType() => globalContext.GetIntType();
    public IType GetStringType() => globalContext.GetStringType();
    public IType GetListType(IType itemType) => globalContext.GetListType(itemType);
    public IType GetListIterType(IType itemType) => globalContext.GetListIterType(itemType);

    public IType GetExpType(R.Exp exp) => globalContext.GetExpType(exp);
    public IType GetExpType(ResolvedExp reExp) => globalContext.GetExpType(reExp);

    public bool IsListType(IType type, [NotNullWhen(returnValue: true)] out IType? itemType) => globalContext.IsListType(type, out itemType);
    public bool IsSeqType(IType type, [NotNullWhen(returnValue: true)] out IType? itemType) => globalContext.IsSeqType(type, out itemType);

    public ImmutableArray<InternalBinaryOperatorInfo> GetBinaryOpInfos(S.BinaryOpKind kind) => globalContext.GetBinaryOpInfos(kind);
    public void AddBody(IFuncDeclSymbol symbol, ImmutableArray<R.Stmt> body) => globalContext.AddBody(symbol, body);
}