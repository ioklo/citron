using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Citron.Collections;

namespace Citron.Infra
{
    // 주의: T가 struct일 경우 T?가 Nullable<T>를 의미하지 않기 때문에, Candidate<MyStruct?>처럼 ?를 사용해 줘야 제대로 동작한다
    public struct Candidates<T>
    {
        bool bOneAssigned;
        T? one;           // 한개까지는 여기에 저장한다
        List<T>? rests;   // 여러개일 경우 one을 포함해서 여기에 저장한다

        public void Clear()
        {
            bOneAssigned = false;
            one = default;
            rests?.Clear();
        }

        public void Add(T item)
        {
            if (!bOneAssigned)
            {
                one = item;
                bOneAssigned = true;
            }
            else if (rests != null)
            {
                rests.Add(item);
            }
            else
            {
                rests = new List<T>() { item };
            }
        }

        public UniqueQueryResult<T> GetUniqueResult()
        {
            if (!bOneAssigned)
                return UniqueQueryResult<T>.NotFound();

            if (rests == null || rests.Count() == 0)
            {
                Debug.Assert(one != null);
                return UniqueQueryResult<T>.Found(one);
            }

            var count = GetCount();
            var builder = ImmutableArray.CreateBuilder<T>(count);
            for (int i = 0; i < count; i++)
                builder.Add(GetAt(i));

            return UniqueQueryResult<T>.MultipleError(builder.MoveToImmutable());
        }

        public bool ContainsItem()
        {
            return bOneAssigned;
        }

        public int GetCount()
        {
            if (rests != null && rests.Count () != 0)
                return rests.Count + 1;
            else if (bOneAssigned)
                return 1;
            else
                return 0;
        }
        
        public T GetAt(int i)
        {
            if (i == 0)
                return one!;
            else
                return rests![i - 1];
        }
    }
}
