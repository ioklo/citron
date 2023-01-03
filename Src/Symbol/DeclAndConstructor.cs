using Citron.Collections;
using System;

namespace Citron.Symbol
{
    public class DeclAndConstructor<TDeclSymbol, TSymbol>
    {
        TDeclSymbol decl;
        Func<ImmutableArray<IType>, TSymbol> funcConstructor;

        public DeclAndConstructor(TDeclSymbol decl, Func<ImmutableArray<IType>, TSymbol> funcConstructor)
        {
            this.decl = decl;
            this.funcConstructor = funcConstructor;
        }

        public TDeclSymbol GetDecl() { return decl; }
        public TSymbol MakeSymbol(ImmutableArray<IType> typeArgs) { return funcConstructor.Invoke(typeArgs); }
    }
}