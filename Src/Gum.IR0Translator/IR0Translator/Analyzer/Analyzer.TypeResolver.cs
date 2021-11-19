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
            public abstract void AddConstraint(TypeValue x, TypeValue y);
            public abstract ImmutableArray<TypeValue> Resolve();
        }

        // do nothing
        class NullTypeResolver : TypeResolver
        {
            ImmutableArray<TypeValue> typeArgs;
            public NullTypeResolver(ImmutableArray<TypeValue> typeArgs) { this.typeArgs = typeArgs; }

            public override void AddConstraint(TypeValue x, TypeValue y) { }
            public override ImmutableArray<TypeValue> Resolve() => typeArgs;
        }
    }
}
