using Citron.Collections;
using System;
using M = Citron.CompileTime;

namespace Citron.Analysis
{
    public interface ITypeDeclSymbol : IDeclSymbolNode
    {
        void Apply(ITypeDeclSymbolVisitor visitor);
    }
}