using Citron.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Infra
{
    public static class Misc
    {
        public static ImmutableArray<T> Arr<T>() => ImmutableArray<T>.Empty;
        public static ImmutableArray<T> Arr<T>(T e) => ImmutableArray.Create(e);
        public static ImmutableArray<T> Arr<T>(T e1, T e2) => ImmutableArray.Create(e1, e2);
        public static ImmutableArray<T> Arr<T>(params T[] elems) => ImmutableArray.Create(elems);
    }

    public interface IHolder<out TValue>
    {
        public TValue GetValue();
    }

    // lazy evaluation for circular dependency
    public class Holder<TValue> : IHolder<TValue>
    {
        TValue? value;

        public Holder()
        {
            this.value = default;
        }

        public Holder(TValue value)
        {
            this.value = value;
        }

        public TValue GetValue() 
        { 
            return value!; 
        }

        public void SetValue(TValue value) 
        {
            this.value = value; 
        }
    }
}
