using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Infra
{
    public static class Misc
    {
        public static ImmutableArray<T> Arr<T>() => ImmutableArray<T>.Empty;
        public static ImmutableArray<T> Arr<T>(T e) => ImmutableArray.Create(e);
        public static ImmutableArray<T> Arr<T>(T e1, T e2) => ImmutableArray.Create(e1, e2);
        public static ImmutableArray<T> Arr<T>(params T[] elems) => ImmutableArray.Create(elems);
        
        [Conditional("DEBUG")]
        public static void EnsurePure<T>(T t)
            where T : IPure
        {
            // compile-time check, do nothing.
        }

        public static T PureIdentity<T>(T t)
            where T : IPure
        {
            return t;
        }   
    }
}
