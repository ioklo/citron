using Citron.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Citron.Infra
{
    public static class CollectionExtensions
    {        
        public static IEnumerable<(TFirst, TSecond)> Zip<TFirst, TSecond>(ImmutableArray<TFirst> firsts, ImmutableArray<TSecond> seconds)
        {
            var enumFirst = firsts.GetEnumerator();
            var enumSecond = seconds.GetEnumerator();

            var moveResultFirst = enumFirst.MoveNext();
            var moveResultSecond = enumSecond.MoveNext();

            while (moveResultFirst && moveResultSecond)
            {
                yield return (enumFirst.Current, enumSecond.Current);

                moveResultFirst = enumFirst.MoveNext();
                moveResultSecond = enumSecond.MoveNext();
            }

            if (moveResultFirst || moveResultSecond)
                throw new InvalidOperationException();
        }
    }
}
