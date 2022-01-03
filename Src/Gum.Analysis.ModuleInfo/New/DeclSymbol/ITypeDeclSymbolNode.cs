using Gum.Collections;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public interface ITypeDeclSymbolNode : IDeclSymbolNode
    {
        void Apply(ITypeDeclSymbolNodeVisitor visitor);
    }
}