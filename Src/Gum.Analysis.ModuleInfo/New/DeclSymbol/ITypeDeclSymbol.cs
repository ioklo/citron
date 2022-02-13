using Gum.Collections;
using System;
using M = Gum.CompileTime;

namespace Citron.Analysis
{
    public interface ITypeDeclSymbol : IDeclSymbolNode
    {
        void Apply(ITypeDeclSymbolVisitor visitor);
    }
}