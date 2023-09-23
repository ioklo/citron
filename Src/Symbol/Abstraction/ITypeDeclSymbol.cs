using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;

namespace Citron.Symbol
{
    public interface ITypeDeclSymbol : IDeclSymbolNode, ICyclicEqualityComparableClass<ITypeDeclSymbol>, ISerializable
    {
        IEnumerable<IFuncDeclSymbol> GetFuncs();

        void AcceptTypeDeclSymbolVisitor<TTypeDeclSymbolVisitor>(ref TTypeDeclSymbolVisitor visitor)
            where TTypeDeclSymbolVisitor : struct, ITypeDeclSymbolVisitor;
    }
}