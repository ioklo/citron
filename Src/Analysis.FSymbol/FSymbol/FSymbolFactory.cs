using System;

namespace Citron.Analysis
{
    public class FSymbolFactory
    {
        public LambdaMemberVarFSymbol MakeLambdaMemberVar(LambdaFSymbol outer, LambdaMemberVarFDeclSymbol decl)
        {
            return new LambdaMemberVarFSymbol(this, outer, decl);
        }

        public LambdaFSymbol MakeLambda(ISymbolNode outer, LambdaFDeclSymbol decl)
        {
            return new LambdaFSymbol(this, new FSymbolOuter.Symbol(outer), decl);
        }

        public LambdaFSymbol MakeLambda(FSymbolOuter outer, LambdaFDeclSymbol decl)
        {
            return new LambdaFSymbol(this, outer, decl);
        }
    }
}