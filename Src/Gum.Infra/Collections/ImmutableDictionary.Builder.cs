using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Collections
{
    public partial struct ImmutableDictionary<TKey, TValue>
        where TKey : notnull
    {
        public struct Builder
        {
            System.Collections.Immutable.ImmutableDictionary<TKey, TValue>.Builder builder;

            public Builder(System.Collections.Immutable.ImmutableDictionary<TKey, TValue>.Builder builder)
            {
                this.builder = builder;
            }

            public void Add(TKey key, TValue value)
            {
                builder.Add(key, value);
            }

            public ImmutableDictionary<TKey, TValue> ToImmutable()
            {
                return new ImmutableDictionary<TKey, TValue>(builder.ToImmutable());
            }
        }
    }
}
