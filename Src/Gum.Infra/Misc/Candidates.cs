using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Misc
{
    public struct Candidates<T> where T : notnull
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
    }
}
