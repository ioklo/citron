using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL.Commands
{
    public class Move : ICommand
    {
        public int Dest { get; private set; }
        public IValue Value { get; private set; }

        public Move(int dest, IValue val)
        {
            Dest = dest;
            Value = val;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
