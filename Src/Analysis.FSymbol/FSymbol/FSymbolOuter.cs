using System;

namespace Citron.Analysis
{
    public abstract record FSymbolOuter
    {
        public abstract FSymbolOuter Apply(TypeEnv typeEnv);
        public abstract TypeEnv GetTypeEnv();        


        public record Symbol(ISymbolNode SymbolNode) : FSymbolOuter
        {
            public override FSymbolOuter Apply(TypeEnv typeEnv)
            {
                var appliedSymbolNode = SymbolNode.Apply(typeEnv);
                return new Symbol(appliedSymbolNode);
            }

            public override TypeEnv GetTypeEnv()
            {
                return SymbolNode.GetTypeEnv();
            }
        }

        public record FSymbol(IFSymbolNode FSymbolNode) : FSymbolOuter
        {
            public override FSymbolOuter Apply(TypeEnv typeEnv)
            {
                var appliedFSymbolNode = FSymbolNode.Apply(typeEnv);
                return new FSymbol(appliedFSymbolNode);            
            }

            public override TypeEnv GetTypeEnv()
            {
                return FSymbolNode.GetTypeEnv();
            }
        }
    }
}