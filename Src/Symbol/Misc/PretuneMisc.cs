using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Symbol
{
    [CustomEqualityComparer(typeof(List<>), typeof(Dictionary<,>))]
    class CustomEqualityComparer
    {
        public static bool Equals<T>(List<T> x, List<T> y)
            => x.SequenceEqual(y); // NOTICE: List의 List는 안된다;

        public static int GetHashCode<T>(List<T> obj)
        {
            var hashCode = new HashCode();
            foreach (var elem in obj)
                hashCode.Add(elem == null ? 0 : elem.GetHashCode());
            return hashCode.ToHashCode();
        }

        public static bool Equals<TKey, TValue>(Dictionary<TKey, TValue> x, Dictionary<TKey, TValue> y)
            where TKey : notnull
        {
            if (x == y) return true; // 
            if (x.Count != y.Count) return false;

            foreach(var (keyX, valueX) in x)
            {   
                if (!y.TryGetValue(keyX, out var valueY)) return false;

                // TValue가 List나 Dictionary면 안된다;
                if (!EqualityComparer<TValue>.Default.Equals(valueX, valueY))
                    return false;
            }

            return true;
        }
            

        public static int GetHashCode<TKey, TValue>(Dictionary<TKey, TValue> obj)
            where TKey : notnull
        {
            var hashCode = new HashCode();
            foreach (var value in obj.Values)
                hashCode.Add(value == null ? 0 : value.GetHashCode());
            return hashCode.ToHashCode();
        }
    }
}
