using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Citron.Symbol
{
    public struct LambdaDeclSymbolComponent<TOuterDeclSymbol> : ICyclicEqualityComparableStruct<LambdaDeclSymbolComponent<TOuterDeclSymbol>>
        where TOuterDeclSymbol : class, IFuncDeclSymbol, ICyclicEqualityComparableClass<TOuterDeclSymbol>
    {
        TOuterDeclSymbol outer;
        List<LambdaDeclSymbol> lambdaDecls;        

        public LambdaDeclSymbolComponent(TOuterDeclSymbol outer)
        {
            this.outer = outer;
            this.lambdaDecls = new List<LambdaDeclSymbol>();
        }

        public IEnumerable<LambdaDeclSymbol> GetLambdas()
        {
            return lambdaDecls;
        }

        public void AddLambda(LambdaDeclSymbol lambdaDecl)
        {
            Debug.Assert(ReferenceEquals(lambdaDecl.GetOuterDeclNode(), outer));
            lambdaDecls.Add(lambdaDecl);
        }

        bool ICyclicEqualityComparableStruct<LambdaDeclSymbolComponent<TOuterDeclSymbol>>.CyclicEquals(ref LambdaDeclSymbolComponent<TOuterDeclSymbol> other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!lambdaDecls.CyclicEqualsClassItem(other.lambdaDecls, ref context))
                return false;

            return true;
        }
    }
}