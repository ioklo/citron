using Citron.Collections;
using Citron.Infra;
using System;

namespace Citron.Symbol
{
    public interface ITypeDeclSymbol : IDeclSymbolNode, ICyclicEqualityComparableClass<ITypeDeclSymbol>
    {
        void Accept(ITypeDeclSymbolVisitor visitor);
    }
}