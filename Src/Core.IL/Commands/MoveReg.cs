using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL.Commands
{
    public class MoveReg : ICommand
    {
        public int Dest { get; private set; }
        public int Src { get; private set; }

        public MoveReg(int dest, int src)
        {
            Dest = dest;
            Src = src;            
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
