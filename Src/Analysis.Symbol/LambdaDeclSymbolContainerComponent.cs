using Citron.Collections;
using System;
using System.Collections.Generic;

namespace Citron.Analysis
{
    public struct LambdaDeclSymbolContainerComponent
    {
        List<LambdaDeclSymbol> lambdaDecls;        

        public LambdaDeclSymbolContainerComponent(ImmutableArray<LambdaDeclSymbol> lambdaDecls)
        {
            this.lambdaDecls = new List<LambdaDeclSymbol>(lambdaDecls.AsEnumerable());
        }

        public void AddLambda(LambdaDeclSymbol lambdaDecl)
        {
            lambdaDecls.Add(lambdaDecl);
        }
    }
}