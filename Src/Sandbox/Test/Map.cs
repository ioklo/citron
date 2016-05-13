using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Test
{
    class StringMap<T>
    {
        List<Tuple<string, T>> data = new List<Tuple<string, T>>();
        Stack<int> Indices = new Stack<int>();

        public void Add(string key, T value)
        {
            data.Add(Tuple.Create(key, value));
        }

        public bool Find(string key, out T value)
        {
            for (int t = data.Count - 1; 0 <= t; t--)
            {
                if( data[t].Item1 == key )
                {
                    value = data[t].Item2;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        public void Push()
        {
            Indices.Push(data.Count);
        }

        public void Pop()
        {
            int index = Indices.Pop();
            data.RemoveRange(index, data.Count - index);
        }
    }
}
