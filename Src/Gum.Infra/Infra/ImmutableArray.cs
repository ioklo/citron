using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Infra
{
    public struct ImmutableArrayBuilder<T>
    {
        System.Collections.Immutable.ImmutableArray<T>.Builder builder;

        public ImmutableArrayBuilder(System.Collections.Immutable.ImmutableArray<T>.Builder builder)
        {
            this.builder = builder;
        }

        public void Add(T item)
        {
            builder.Add(item);
        }

        public ImmutableArray<T> ToImmutable()
        {
            return new ImmutableArray<T>(builder.ToImmutable());
        }

        public ImmutableArray<T> MoveToImmutable()
        {
            return new ImmutableArray<T>(builder.MoveToImmutable());
        }
    }

    public static class ImmutableArray
    {
        public static ImmutableArrayBuilder<T> CreateBuilder<T>()
        {

        }

        public static ImmutableArrayBuilder<T> CreateBuilder<T>(int length)
        {

        }

        public static ImmutableArrayBuilder<TElem> CreateRange<TElem, TResult>(ImmutableArray<TElem> source, Func<TElem, TResult>)
        {
        }
    }

    public struct ImmutableArray<T>
    {
        internal System.Collections.Immutable.ImmutableArray<T> array;

        public ImmutableArray(System.Collections.Immutable.ImmutableArray<T> array)
        {
            this.array = array;
        }
    }

    public static class ImmutableArrayExtensions
    {
        public static IEnumerable<TResult> Select<TElem, TResult>(this Gum.Infra.ImmutableArray<TElem> array, Func<TElem, TResult> selector)
        {
            return System.Linq.ImmutableArrayExtensions.Select(array.array, selector);
        }
    }
}

