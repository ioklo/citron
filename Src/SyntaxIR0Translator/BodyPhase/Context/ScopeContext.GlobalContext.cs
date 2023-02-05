using System.Diagnostics.CodeAnalysis;
using Citron.Collections;
using Citron.Symbol;
using S = Citron.Syntax;

namespace Citron.Analysis;

// GlobalContext redirection
partial class ScopeContext
{   
    [DoesNotReturn]
    public void AddFatalError(SyntaxAnalysisErrorCode code, S.ISyntaxNode node) => globalContext.AddFatalError(code, node);
    public void AddError(SyntaxAnalysisErrorCode code, S.ISyntaxNode node) => globalContext.AddError(code, node);
    public InternalGlobalVarInfo? GetInternalGlobalVarInfo(string idName) => globalContext.GetInternalGlobalVarInfo(idName);

    public IType GetVoidType() => globalContext.GetVoidType();
    public IType GetBoolType() => globalContext.GetBoolType();
    public IType GetIntType() => globalContext.GetIntType();
    public IType GetStringType() => globalContext.GetStringType();
    public IType GetListType(IType itemType) => globalContext.GetListType(itemType);
    public IType GetListIterType(IType itemType) => globalContext.GetListIterType(itemType);

    public bool IsVoidType(IType type) => globalContext.IsVoidType(type);
    public bool IsBoolType(IType type) => globalContext.IsBoolType(type);
    public bool IsIntType(IType type) => globalContext.IsIntType(type);
    public bool IsStringType(IType type) => globalContext.IsStringType(type);
    public bool IsListType(IType type, [NotNullWhen(returnValue: true)] out IType? itemType) => globalContext.IsListType(type, out itemType);
    public bool IsSeqType(IType type, [NotNullWhen(returnValue: true)] out IType? itemType) => globalContext.IsSeqType(type, out itemType);

    public ImmutableArray<InternalBinaryOperatorInfo> GetBinaryOpInfos(S.BinaryOpKind kind) => globalContext.GetBinaryOpInfos(kind);
    public IType MakeType(S.TypeExp typeExp) => globalContext.MakeType(typeExp);
}