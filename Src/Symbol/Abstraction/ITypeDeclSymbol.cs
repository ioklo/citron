using Citron.Collections;
using System;

namespace Citron.Symbol
{
    public interface ITypeDeclSymbol : IDeclSymbolNode
    {
        void Apply(ITypeDeclSymbolVisitor visitor);
    }
}