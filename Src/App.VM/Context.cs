using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gum.Core.IL;

namespace Gum.App.VM
{
    public class Context
    {
        Stack<StackFrame> frames = new Stack<StackFrame>();

        public StackFrame CurFrame { get; private set; }
        public int Point
        {
            get { return CurFrame.Pos; }
            set { CurFrame.Pos = value; }
        }

        public Context()
        {
            frames.Push(new StackFrame(0, null));
            CurFrame = frames.Peek();
            Point = 0;
        }

        public void PushFrame(int locals, FuncInfo fi)
        {
            frames.Push(new StackFrame(locals, fi));
            CurFrame = frames.Peek();
            Point = 0;
        }

        public void PopFrame()
        {
            frames.Pop();
            CurFrame = frames.Peek();
        }
    }
}
