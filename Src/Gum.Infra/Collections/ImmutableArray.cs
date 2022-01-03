using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Collections
{
    public static class ImmutableArray
    {
        public static ImmutableArray<T>.Builder CreateBuilder<T>()
        {
            return new ImmutableArray<T>.Builder(System.Collections.Immutable.ImmutableArray.CreateBuilder<T>());
        }

        public static ImmutableArray<T>.Builder CreateBuilder<T>(int length)
        {
            return new ImmutableArray<T>.Builder(System.Collections.Immutable.ImmutableArray.CreateBuilder<T>(length));
        }

        public static ImmutableArray<TResult> CreateRange<TElem, TResult>(ImmutableArray<TElem> source, Func<TElem, TResult> selector)
        {
            // default 거르기
            if (source.array.IsDefault) return ImmutableArray<TResult>.Empty;

            return new ImmutableArray<TResult>(System.Collections.Immutable.ImmutableArray.CreateRange(source.array, selector));
        }

        public static ImmutableArray<T> Create<T>(T item)
        {
            return new ImmutableArray<T>(System.Collections.Immutable.ImmutableArray.Create(item));
        }

        public static ImmutableArray<T> Create<T>(T item1, T item2)
        {
            return new ImmutableArray<T>(System.Collections.Immutable.ImmutableArray.Create(item1, item2));
        }
        
        public static ImmutableArray<T> Create<T>(ImmutableArray<T> items, int start, int length)
        {
            return new ImmutableArray<T>(System.Collections.Immutable.ImmutableArray.Create(items.array, start, length));
        }

        public static ImmutableArray<T> Create<T>(params T[]? items)
        {
            return new ImmutableArray<T>(System.Collections.Immutable.ImmutableArray.Create(items));
        }

        public static ImmutableArray<T> ToImmutableArray<T>(this T[] items)
        {
            return new ImmutableArray<T>(System.Collections.Immutable.ImmutableArray.ToImmutableArray(items));
        }

        public static ImmutableArray<T> ToImmutableArray<T>(this IEnumerable<T> items)
        {
            return new ImmutableArray<T>(System.Collections.Immutable.ImmutableArray.ToImmutableArray(items));
        }
    }

    public partial struct ImmutableArray<T> : IEquatable<ImmutableArray<T>>
    {
        public static readonly ImmutableArray<T> Empty;
        internal System.Collections.Immutable.ImmutableArray<T> array;

        public ImmutableArray(System.Collections.Immutable.ImmutableArray<T> array)
        {   
            this.array = array;            
        }
        
        public bool IsEmpty => array.IsDefaultOrEmpty;
        public int Length => array.IsDefault ? 0 : array.Length;

        // no heap allocation
        public IEnumerable<T> AsEnumerable() { if (array.IsDefault) return Enumerable.Empty<T>(); else return array; }

        public static ImmutableArray<T> CastUp<TDerived>(ImmutableArray<TDerived> items) where TDerived : class, T
        {
            return new ImmutableArray<T>(System.Collections.Immutable.ImmutableArray<T>.CastUp(items.array));
        }

        public Enumerator GetEnumerator()
        {
            if (array.IsDefault) return default;

            return new Enumerator(array.GetEnumerator());
        }

        public ImmutableArray<T> Add(T item)
        {
            if (array.IsDefault)
                return new ImmutableArray<T>(System.Collections.Immutable.ImmutableArray<T>.Empty.Add(item));

            return new ImmutableArray<T>(array.Add(item));
        }        

        public T this[int index] => array[index];

        public override bool Equals(object? obj) => obj is ImmutableArray<T> other && Equals(other);
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            if (array.IsDefault) return hashCode.ToHashCode();

            for (int i = 0; i < array.Length; i++)
            {
                T item = array[i];
                hashCode.Add(item == null ? 0 : item.GetHashCode());
            }
            return hashCode.ToHashCode();
        }

        public bool Equals(ImmutableArray<T> other)
        {
            if (IsEmpty && other.IsEmpty) return true;
            if (IsEmpty || other.IsEmpty) return false;

            return System.Linq.ImmutableArrayExtensions.SequenceEqual(array, other.array);
        }

        public bool Contains(T item)
        {
            if (array.IsDefault) return false;
            return array.Contains(item);
        }

        public override string ToString()
        {
            // [ ... , ]

            if (array.IsDefault) return "[]";

            var sb = new StringBuilder();
            sb.Append('[');

            bool bFirst = true;
            foreach (var item in array)
            {
                if (bFirst) bFirst = false;
                else sb.Append(", ");

                sb.Append(item);                
            }

            sb.Append(']');

            return sb.ToString();
        }

        public ImmutableArray<T> AddRange(ImmutableArray<T> items)
        {
            if (array.IsDefault)
                return new ImmutableArray<T>(System.Collections.Immutable.ImmutableArray<T>.Empty.AddRange(items.array));

            return new ImmutableArray<T>(array.AddRange(items.array));
        }
    }
}

