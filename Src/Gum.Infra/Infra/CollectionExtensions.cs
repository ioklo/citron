using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Gum.Infra
{
    public static class CollectionExtensions
    {
        public static ImmutableDictionary<TKey, TValue> ToImmutableWithComparer<TKey, TValue>(this Dictionary<TKey, TValue> dict)
            where TKey : notnull
        {
            return dict.ToImmutableDictionary(dict.Comparer);
        }           

        public static IEnumerable<(TFirst, TSecond)> Zip<TFirst, TSecond>(ImmutableArray<TFirst> firsts, ImmutableArray<TSecond> seconds)
        {
            var enumFirst = firsts.GetEnumerator();
            var enumSecond = seconds.GetEnumerator();

            var moveResultFirst = enumFirst.MoveNext();
            var moveResultSecond = enumSecond.MoveNext();

            while (moveResultFirst && moveResultSecond)
            {
                yield return (enumFirst.Current, enumSecond.Current);

                moveResultFirst = enumFirst.MoveNext();
                moveResultSecond = enumSecond.MoveNext();
            }

            if (moveResultFirst || moveResultSecond)
                throw new InvalidOperationException();
        }
    }
}
