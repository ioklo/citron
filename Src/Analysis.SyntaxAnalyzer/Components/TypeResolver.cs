using Citron.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Analysis
{    
    abstract class TypeResolver
    {
        public abstract void AddConstraint(ITypeSymbol x, ITypeSymbol y);
        public abstract ImmutableArray<ITypeSymbol> Resolve();
    }

    // do nothing
    class NullTypeResolver : TypeResolver
    {
        ImmutableArray<ITypeSymbol> typeArgs;
        public NullTypeResolver(ImmutableArray<ITypeSymbol> typeArgs) { this.typeArgs = typeArgs; }

        public override void AddConstraint(ITypeSymbol x, ITypeSymbol y) { }
        public override ImmutableArray<ITypeSymbol> Resolve() => typeArgs;
    }    
}
