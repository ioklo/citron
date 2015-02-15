using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Core.IL;

namespace Gum.App.VM
{
    // State = 현재 실행 위치 * Memory(Location -> Handle) * Local Varaibles..(int -> Value) * Call Stacks
    public class State
    {
        public StackFrame CurFrame { get { return Context.CurFrame; } }
        public Context Context { get; set; }
        public bool Pause { get; set; }
        
        public State()
        {
            Context = null;
        }

        public void Push(object val)
        {
            CurFrame.Stack.Push(val);
        }

        public void Dup()
        {
            CurFrame.Stack.Push(CurFrame.Stack.Peek());
        }

        public object Pop()
        {
            return CurFrame.Stack.Pop();
        }
        
        public void SetLocalValue(int reg, object value)
        {
            CurFrame.Locals[reg] = value;
        }

        public object GetLocalValue(int reg)
        {
            return CurFrame.Locals[reg];
        }

        public void PushFrame(int locals, FuncInfo fi)
        {
            Context.PushFrame(locals, fi);
        }

        public void PopFrame()
        {
            Context.PopFrame();
        }

        public void Jump(int i)
        {
            Context.Point = i;
        }
    }
}
