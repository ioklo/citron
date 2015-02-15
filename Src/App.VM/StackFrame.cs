using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Core.IL;

namespace Gum.App.VM
{
    // 현재 함수에서의 컨텍스트 
    public class StackFrame
    {
        public FuncInfo Func { get; private set; }

        // 함수에서의 실행 위치
        public int Pos { get; internal set; }             // 현재 실행 위치

        // 레지스터들의 위치
        public List<object> Locals { get; private set; }

        // expression들이 들어갈 stack
        public Stack<object> Stack { get; private set; }

        public StackFrame(int locals, FuncInfo fi)
        {
            Stack = new Stack<object>();
            Locals = new List<object>(locals);
            Locals.AddRange(Enumerable.Repeat<object>(null, locals));
            Func = fi;
        }
    }
}
