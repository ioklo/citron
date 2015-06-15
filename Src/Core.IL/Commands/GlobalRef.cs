using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL.Commands
{
    // Global 변수의 값을 
    public class GlobalRef : ICommand
    {
        public int DestReg { get; private set; }
        public int Index { get; private set; }

        public GlobalRef(int destReg, int index)
        {
            DestReg = destReg;
            Index = index;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
