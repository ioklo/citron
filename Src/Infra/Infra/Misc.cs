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
        public event Action<TValue> OnSetValue;
    }

    // lazy evaluation for circular dependency
    public class Holder<TValue> : IHolder<TValue>
    {
        TValue? value;
        bool bSet;

        Action<TValue>? onSetValueDelegate;

        public event Action<TValue> OnSetValue
        {
            add
            {
                if (bSet)
                    value.Invoke(this.value!);
                else
                    onSetValueDelegate += value;
            }

            remove
            {
                Debug.Assert(!bSet); // 세팅이 안되었을 때만 유효하다

                if (onSetValueDelegate != null)
                    onSetValueDelegate -= value;
            }
        }

        public Holder()
        {
            this.value = default;
            this.bSet = false;
            this.onSetValueDelegate = null;
        }

        public Holder(TValue? value)
        {
            this.value = value;
            this.bSet = true;
            this.onSetValueDelegate = null;
        }        

        public TValue GetValue() 
        {
            Debug.Assert(bSet);
            return value!; 
        }

        public void SetValue(TValue value) 
        {
            Debug.Assert(!bSet);

            this.value = value;
            bSet = true;

            if (onSetValueDelegate != null)
            {
                onSetValueDelegate.Invoke(value);
                onSetValueDelegate = null;
            }
        }
    }

    public static class HolderExtensions
    {
        public static IHolder<TValue> ToHolder<TValue>(this TValue value)
        {
            return new Holder<TValue>(value);
        }
    }
}
