using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL.Commands
{
    public class IfNotJump : ICommand
    {
        public int Cond { get; private set; }
        public int Block { get; private set; }

        public IfNotJump(int cond, int block)
        {
            Cond = cond;
            Block = block;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
