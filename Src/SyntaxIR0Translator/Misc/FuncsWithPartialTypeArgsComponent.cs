using Citron.Collections;
using Citron.Symbol;
using Pretune;

namespace Citron.Analysis;

[AutoConstructor]
partial struct FuncsWithPartialTypeArgsComponent<TDeclSymbol, TSymbol>
    where TDeclSymbol : IFuncDeclSymbol
    where TSymbol : IFuncSymbol
{
    ImmutableArray<(ISymbolNode Outer, TDeclSymbol DeclSymbol)> outerAndDeclSymbols;
    ImmutableArray<IType> partialTypeArgs;

    public int GetCount() { return outerAndDeclSymbols.Length; }

    public TDeclSymbol GetDecl(int i) { return outerAndDeclSymbols[i].DeclSymbol; }

    public TypeEnv GetOuterTypeEnv(int i) { return outerAndDeclSymbols[i].Outer.GetTypeEnv(); }

    public ImmutableArray<IType> GetPartialTypeArgs() { return partialTypeArgs; }

    public TSymbol MakeSymbol(int i, ImmutableArray<IType> typeArgs, ScopeContext context)
    {
        var (outer, declSymbol) = outerAndDeclSymbols[i];
        return (TSymbol)context.InstantiateSymbol(outer, declSymbol, typeArgs);
    }
}

