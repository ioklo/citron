using Citron.Collections;
using System;

namespace Citron.Analysis
{
    public class DeclAndConstructor<TDeclSymbol, TSymbol>
    {
        TDeclSymbol decl;
        Func<ImmutableArray<ITypeSymbol>, TSymbol> funcConstructor;

        public DeclAndConstructor(TDeclSymbol decl, Func<ImmutableArray<ITypeSymbol>, TSymbol> funcConstructor)
        {
            this.decl = decl;
            this.funcConstructor = funcConstructor;
        }

        public TDeclSymbol GetDecl() { return decl; }
        public TSymbol MakeSymbol(ImmutableArray<ITypeSymbol> typeArgs) { return funcConstructor.Invoke(typeArgs); }
    }
}