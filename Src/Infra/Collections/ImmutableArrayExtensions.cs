using System;
using System.Collections.Generic;
using System.Linq;

namespace Citron.Collections
{
    public static class ImmutableArrayExtensions
    {
        public static IEnumerable<TResult> Select<TElem, TResult>(this ImmutableArray<TElem> array, Func<TElem, TResult> selector)
        {
            if (array.array.IsDefault)
                return Enumerable.Empty<TResult>();

            return System.Linq.ImmutableArrayExtensions.Select(array.array, selector);
        }

        public static IEnumerable<TElem> Where<TElem>(this ImmutableArray<TElem> array, Func<TElem, bool> predicate)
        {
            if (array.array.IsDefault)
                return Enumerable.Empty<TElem>();

            return System.Linq.ImmutableArrayExtensions.Where(array.array, predicate);
        }
        
        public static bool SequenceEqual<T>(this ImmutableArray<T> x, ImmutableArray<T> y, IEqualityComparer<T> comparer)
        {
            if (x.IsEmpty && y.IsEmpty) return true;
            if (x.IsEmpty || y.IsEmpty) return false;

            return System.Linq.ImmutableArrayExtensions.SequenceEqual(x.array, y.array, comparer);
        }

        public static T? FirstOrDefault<T>(this ImmutableArray<T> array, Func<T, bool> predicate)
        {
            return System.Linq.ImmutableArrayExtensions.FirstOrDefault(array.array, predicate);
        }
    }
}

