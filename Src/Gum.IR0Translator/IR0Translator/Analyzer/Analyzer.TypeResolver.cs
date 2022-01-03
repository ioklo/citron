using Gum.Analysis;
using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        abstract class TypeResolver
        {
            public abstract void AddConstraint(TypeSymbol x, TypeSymbol y);
            public abstract ImmutableArray<TypeSymbol> Resolve();
        }

        // do nothing
        class NullTypeResolver : TypeResolver
        {
            ImmutableArray<ITypeSymbolNode> typeArgs;
            public NullTypeResolver(ImmutableArray<ITypeSymbolNode> typeArgs) { this.typeArgs = typeArgs; }

            public override void AddConstraint(TypeSymbol x, TypeSymbol y) { }
            public override ImmutableArray<TypeSymbol> Resolve() => typeArgs;
        }
    }
}
