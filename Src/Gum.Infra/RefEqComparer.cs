using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Gum.Infra
{
    // ReferenceEqualityComparer는 .net 5부터 지원
    public class RefEqComparer<T> : IEqualityComparer<T> where T : class
    {
        public static readonly RefEqComparer<T> Instance = new RefEqComparer<T>();

        private RefEqComparer() { }

        public bool Equals(T? x, T? y)
        {
            return Object.ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

}
