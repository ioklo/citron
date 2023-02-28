using Citron.Collections;
using Citron.Symbol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Analysis
{    
    abstract class TypeResolver
    {
        public abstract void AddConstraint(IType x, IType y);
        public abstract ImmutableArray<IType > Resolve();
    }

    // do nothing
    class NullTypeResolver : TypeResolver
    {
        ImmutableArray<IType> typeArgs;
        public NullTypeResolver(ImmutableArray<IType> typeArgs) { this.typeArgs = typeArgs; }

        public override void AddConstraint(IType x, IType y) { }
        public override ImmutableArray<IType> Resolve() => typeArgs;
    }    
}
