using System;
using System.Collections.Generic;
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
    }
}
