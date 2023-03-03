using Citron.Collections;
using Citron.Infra;
using System;

namespace Citron.Symbol
{
    public interface ITypeDeclSymbol : IDeclSymbolNode, ICyclicEqualityComparableClass<ITypeDeclSymbol>, ISerializable
    {
        void AcceptTypeDeclSymbolVisitor<TTypeDeclSymbolVisitor>(ref TTypeDeclSymbolVisitor visitor)
            where TTypeDeclSymbolVisitor : struct, ITypeDeclSymbolVisitor;
    }
}