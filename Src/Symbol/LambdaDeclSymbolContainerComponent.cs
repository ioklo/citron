using Citron.Collections;
using System;
using System.Collections.Generic;

namespace Citron.Symbol
{
    public struct LambdaDeclSymbolContainerComponent
    {
        List<LambdaDeclSymbol> lambdaDecls;        

        public LambdaDeclSymbolContainerComponent(ImmutableArray<LambdaDeclSymbol> lambdaDecls)
        {
            this.lambdaDecls = new List<LambdaDeclSymbol>(lambdaDecls.AsEnumerable());
        }

        public IEnumerable<LambdaDeclSymbol> GetLambdaDecls()
        {
            return lambdaDecls;
        }

        public void AddLambda(LambdaDeclSymbol lambdaDecl)
        {
            lambdaDecls.Add(lambdaDecl);
        }
    }
}