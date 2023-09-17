using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Infra
{
    public interface ICyclicEqualityComparableStruct<T>
        where T : struct
    {
        bool CyclicEquals(ref T other, ref CyclicEqualityCompareContext context);
    }

    public interface ICyclicEqualityComparableClass<T>
        where T : class
    {
        bool CyclicEquals(T other, ref CyclicEqualityCompareContext context);
    }

    public struct CyclicEqualityCompareContext
    {
        HashSet<(object, object)> visited; // 비교하려는 대상을 visited에 넣고, 추후에 같다고 이야기를 한다 (preventing reenterancy)

        public CyclicEqualityCompareContext()
        {
            visited = new HashSet<(object, object)>();
        }

        public bool CompareClass<T>(T? x, T? y)
            where T : class, ICyclicEqualityComparableClass<T>
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            if (object.ReferenceEquals(x, y)) return true;

            if (visited.Contains((x, y)) || visited.Contains((y, x)))
                return true;

            // 없었다면 바로 추가
            visited.Add((x, y));

            if (CyclicEqualityComparer.CyclicEquals(x, y, ref this))
            {   
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CompareStructRef<T>(ref T x, ref T y)
            where T : struct, ICyclicEqualityComparableStruct<T>
        {
            return x.CyclicEquals(ref y, ref this);
        }
    }

    public static class CyclicEqualityComparer
    {
        // entry for class
        //public static bool CyclicEquals<T>(T? x, T? y)
        //    where T : class, ICyclicEqualityComparableClass<T>
        //{
        //    if (x == null && y == null) return true;
        //    if (x == null || y == null) return false;

        //    var context = new CyclicEqualityCompareContext();
        //    return x.CyclicEquals(y, ref context);
        //}

        public static bool CyclicEquals<T>(this T? x, T? y, ref CyclicEqualityCompareContext context)
            where T : class, ICyclicEqualityComparableClass<T>
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.CyclicEquals(y, ref context);
        }

        // entry for struct
        //public static bool CyclicEquals<T>(this ref T x, ref T y)
        //    where T : struct, ICyclicEqualityComparableStruct<T>
        //{   
        //    var context = new CyclicEqualityCompareContext();
        //    return x.CyclicEquals(ref y, ref context);
        //}

        // entry for nullable struct
        //public static bool CyclicEquals<T>(this ref T? x, ref T? y)
        //    where T : struct, ICyclicEqualityComparableStruct<T>
        //{
        //    if (x == null && y == null) return true;
        //    if (x == null || y == null) return false;

        //    var context = new CyclicEqualityCompareContext();

        //    var valueY = y.Value; // NOTICE: copy!!!, Nullable<T>.Value는 프로퍼티라서 ref참조가 안된다
        //    return x.Value.CyclicEquals(ref valueY, ref context);
        //}

        // sugar
        public static bool CyclicEquals<T>(this ref T x, ref T y, ref CyclicEqualityCompareContext context)
            where T : struct, ICyclicEqualityComparableStruct<T>
        {
            return x.CyclicEquals(ref y, ref context);
        }

        public static bool CyclicEquals<T>(this ref T? x, ref T? y, ref CyclicEqualityCompareContext context)
            where T : struct, ICyclicEqualityComparableStruct<T>
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            var valueY = y.Value;
            return x.Value.CyclicEquals(ref valueY, ref context);
        }

        public static bool CyclicEqualsClassItem<T>(this List<T> x, List<T> y, ref CyclicEqualityCompareContext context)
            where T : class, ICyclicEqualityComparableClass<T>
        {
            int countX = x.Count;
            int countY = y.Count;

            if (countX == 0 && countY == 0) return true;
            if (countX == 0 || countY == 0) return false;

            if (countX != countY) return false;
            
            for(int i = 0; i < countX; i++)
            {
                if (!context.CompareClass(x[i], y[i]))
                    return false;
            }

            return true;
        }

        public static bool CyclicEqualsStructItem<T>(this List<T> x, List<T> y, ref CyclicEqualityCompareContext context)
            where T : struct, ICyclicEqualityComparableStruct<T>
        {
            int countX = x.Count;
            int countY = y.Count;

            if (countX == 0 && countY == 0) return true;
            if (countX == 0 || countY == 0) return false;

            if (countX != countY) return false;

            for (int i = 0; i < countX; i++)
            {
                var valueY = y[i];
                if (!x[i].CyclicEquals(ref valueY, ref context))
                    return false;
            }

            return true;
        }

        //public static bool CyclicEqualsCyclicStructKeyAndClassValue<TKey, TValue>(this Dictionary<TKey, TValue> x, Dictionary<TKey, TValue> y, ref CyclicEqualityCompareContext context)
        //    where TKey : struct, ICyclicEqualityComparableStruct<TKey>
        //    where TValue : class, ICyclicEqualityComparableClass<TValue>
        //{
        //    int countX = x.Count;
        //    int countY = y.Count;

        //    if (countX == 0 && countY == 0) return true;
        //    if (countX == 0 || countY == 0) return false;

        //    if (countX != countY) return false;

        //    // x에서 쓰인 키와 y에서 쓰인 키가 Equals를 충족하지 않는다
        //    foreach (var (keyX, valueX) in x)
        //    {
        //        // keyY를 수동으로 찾아본다 O(n)
        //        TKey? matchedKeyY = null;
        //        foreach (var keyY in y.Keys)
        //        {
        //            var savedKeyY = keyY;
        //            if (keyX.CyclicEquals(ref savedKeyY, ref context))
        //            {
        //                matchedKeyY = keyY;
        //                break;
        //            }
        //        }

        //        // 키를 못찾았으면 같지 않다
        //        if (matchedKeyY == null) return false;

        //        if (!y.TryGetValue(matchedKeyY.Value, out var valueY))
        //            return false;

        //        if (!context.CompareClass(valueX, valueY))
        //            return false;
        //    }

        //    return true;
        //}

        public static bool CyclicEqualsClassValue<TKey, TValue>(this Dictionary<TKey, TValue> x, Dictionary<TKey, TValue> y, ref CyclicEqualityCompareContext context)
            where TKey : notnull
            where TValue : class, ICyclicEqualityComparableClass<TValue>
        {
            int countX = x.Count;
            int countY = y.Count;

            if (countX == 0 && countY == 0) return true;
            if (countX == 0 || countY == 0) return false;

            if (countX != countY) return false;

            // x에서 쓰인 키와 y에서 쓰인 키가 Equals를 충족한다고 본다
            foreach(var (key, valueX) in x)
            {   
                if (!y.TryGetValue(key, out var valueY))
                    return false;

                if (!context.CompareClass(valueX, valueY))
                    return false;
            }

            return true;
        }

        public static bool CyclicEqualsStructValue<TKey, TValue>(this Dictionary<TKey, TValue> x, Dictionary<TKey, TValue> y, ref CyclicEqualityCompareContext context)
            where TKey : notnull
            where TValue : struct, ICyclicEqualityComparableStruct<TValue>
        {
            int countX = x.Count;
            int countY = y.Count;

            if (countX == 0 && countY == 0) return true;
            if (countX == 0 || countY == 0) return false;

            if (countX != countY) return false;

            foreach(var (key, valueX) in x)
            {
                if (!y.TryGetValue(key, out var valueY))
                    return false;

                if (!valueX.CyclicEquals(ref valueY, ref context))
                    return false;
            }

            return true;
        }
    }

}
