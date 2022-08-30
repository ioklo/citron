using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Infra
{
    // 주의: T가 struct일 경우 T?가 Nullable<T>를 의미하지 않기 때문에, Candidate<MyStruct?>처럼 ?를 사용해 줘야 제대로 동작한다
    public struct Candidates<T>
    {   
        T? one;           // 한개까지는 여기에 저장한다
        List<T>? rests;   // 여러개일 경우 one을 포함해서 여기에 저장한다

        public void Add(T item)
        {
            if (rests != null)
                rests.Add(item);

            else if (one != null)
            {
                // rests 만들기
                rests = new List<T>() { one, item };
                one = default;
            }
            else
            {
                one = item;
            }
        }

        // 순서.. Single을 얻어보고, null이면 Empty냐 Multiple이냐 알아본다
        public T? GetSingle() { return one; }
        public bool IsEmpty { get => one == null && rests == null; }
        public bool HasMultiple { get => rests != null; }

        public UniqueQueryResult<T> GetResult()
        {
            if (one != null)            
                return UniqueQueryResults<T>.Found(one);

            if (rests != null)
                return UniqueQueryResults<T>.MultipleError;

            Debug.Assert(one == null && rests == null);
            return UniqueQueryResults<T>.NotFound;
        }
    }
}
