using Citron.Infra;
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

        public static bool CyclicEqualsClassItem<T>(ref this ImmutableArray<T> x, ref ImmutableArray<T> y, ref CyclicEqualityCompareContext context)
            where T : class, ICyclicEqualityComparableClass<T>
        {
            if (x.IsEmpty && y.IsEmpty) return true;
            if (x.IsEmpty || y.IsEmpty) return false;

            if (x.array.Length != y.array.Length) return false;

            int arrayCount = x.array.Length;
            for (int i = 0; i < arrayCount; i++)
            {
                if (!context.CompareClass(x.array[i], y.array[i]))
                    return false;
            }

            return true;
        }

        public static bool CyclicEqualsStructItem<T>(ref this ImmutableArray<T> x, ref ImmutableArray<T> y, ref CyclicEqualityCompareContext context)
            where T : struct, ICyclicEqualityComparableStruct<T>
        {
            if (x.IsEmpty && y.IsEmpty) return true;
            if (x.IsEmpty || y.IsEmpty) return false;

            if (x.array.Length != y.array.Length) return false;

            int arrayCount = x.array.Length;
            for (int i = 0; i < arrayCount; i++)
            {
                var valueY = y.array[i];
                if (!x.array[i].CyclicEquals(ref valueY, ref context))
                    return false;
            }

            return true;
        }
    }
}

