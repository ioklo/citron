using System;
using Citron.Collections;
using Citron.Symbol;

namespace Citron.Analysis;

static class FuncCandidates
{
    public static FuncCandidates<TFuncDeclSymbol, TFuncSymbol> Make<TFuncDeclSymbol, TFuncSymbol>(ISymbolNode outer, int count, Func<int, TFuncDeclSymbol> declSymbolGetter, ImmutableArray<IType> partialTypeArgs)
        where TFuncDeclSymbol : IFuncDeclSymbol
        where TFuncSymbol : IFuncSymbol
    {
        var builder = ImmutableArray.CreateBuilder<(ISymbolNode, TFuncDeclSymbol)>(count);
        for (int i = 0; i < count; i++)
        {
            var declSymbol = declSymbolGetter.Invoke(i);
            builder.Add((outer, declSymbol));
        }

        return new FuncCandidates<TFuncDeclSymbol, TFuncSymbol>(builder.MoveToImmutable(), partialTypeArgs);
    }
}

class FuncCandidates<TFuncDeclSymbol, TFuncSymbol> : IFuncs<TFuncDeclSymbol, TFuncSymbol>
    where TFuncDeclSymbol : IFuncDeclSymbol
    where TFuncSymbol : IFuncSymbol
{
    FuncsWithPartialTypeArgsComponent<TFuncDeclSymbol, TFuncSymbol> component;

    public FuncCandidates(ImmutableArray<(ISymbolNode Outer, TFuncDeclSymbol DeclSymbol)> outerAndDeclSymbols, ImmutableArray<IType> partialTypeArgs)
    {
        component = new FuncsWithPartialTypeArgsComponent<TFuncDeclSymbol, TFuncSymbol>(outerAndDeclSymbols, partialTypeArgs);
    }


    int IFuncs<TFuncDeclSymbol, TFuncSymbol>.GetCount() => component.GetCount();
    TFuncDeclSymbol IFuncs<TFuncDeclSymbol, TFuncSymbol>.GetDecl(int i) => component.GetDecl(i);
    TypeEnv IFuncs<TFuncDeclSymbol, TFuncSymbol>.GetOuterTypeEnv(int i) => component.GetOuterTypeEnv(i);
    ImmutableArray<IType> IFuncs<TFuncDeclSymbol, TFuncSymbol>.GetPartialTypeArgs() => component.GetPartialTypeArgs();
    TFuncSymbol IFuncs<TFuncDeclSymbol, TFuncSymbol>.MakeSymbol(int i, ImmutableArray<IType> typeArgs, ScopeContext context)
        => component.MakeSymbol(i, typeArgs, context);
}
