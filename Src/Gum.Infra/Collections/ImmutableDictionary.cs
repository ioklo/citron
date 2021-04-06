using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Collections
{
    public static class ImmutableDictionary
    {
        public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>()
            where TKey : notnull
        {
            return new ImmutableDictionary<TKey, TValue>.Builder(System.Collections.Immutable.ImmutableDictionary.CreateBuilder<TKey, TValue>());
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
            where TKey : notnull
        {
            return new ImmutableDictionary<TKey, TValue>(
                System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary(source));
        }
    }

    public partial struct ImmutableDictionary<TKey, TValue>
        where TKey : notnull
    {
        public static readonly ImmutableDictionary<TKey, TValue> Empty =
            new ImmutableDictionary<TKey, TValue>(System.Collections.Immutable.ImmutableDictionary<TKey, TValue>.Empty);

        System.Collections.Immutable.ImmutableDictionary<TKey, TValue> dict;

        public ImmutableDictionary(System.Collections.Immutable.ImmutableDictionary<TKey, TValue> dict)
        {
            this.dict = dict;
        }

        public TValue this[TKey key] => dict[key];

        public ImmutableDictionary<TKey, TValue> SetItem(TKey key, TValue value)
        {
            return new ImmutableDictionary<TKey, TValue>(dict.SetItem(key, value));
        }

        public TValue? GetValueOrDefault(TKey key)
        {
            return dict.GetValueOrDefault(key);
        }
    }    
}
