using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Collections
{
    public struct ImmutableHashSet<T>
    {
        System.Collections.Immutable.ImmutableHashSet<T> hashSet;

        public static readonly ImmutableHashSet<T> Empty
            = new ImmutableHashSet<T>(System.Collections.Immutable.ImmutableHashSet<T>.Empty);

        public ImmutableHashSet(System.Collections.Immutable.ImmutableHashSet<T> hashSet)
        {
            this.hashSet = hashSet;
        }
    }
}
