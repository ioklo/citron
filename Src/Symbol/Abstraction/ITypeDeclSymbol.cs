using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;

namespace Citron.Symbol
{
    public interface ITypeDeclSymbol : IDeclSymbolNode, ICyclicEqualityComparableClass<ITypeDeclSymbol>, ISerializable
    {
        new TResult Accept<TTypeDeclSymbolVisitor, TResult>(ref TTypeDeclSymbolVisitor visitor)
            where TTypeDeclSymbolVisitor : struct, ITypeDeclSymbolVisitor<TResult>;
    }
}