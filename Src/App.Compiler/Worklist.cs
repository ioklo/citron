using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.App.Compiler
{
    class Worklist
    {
        public int Count { get { return list.Count; } }
        public List<int> list = new List<int>();

        // 같은게 집어넣어지면 가장 맨 위로
        internal void Push(int p)
        {
            list.RemoveAll(i => i == p);
            list.Add(p);
        }


        // 위에서 하나 뺀다
        internal int Pop()
        {
            int ret = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return ret;
        }
    }
}
