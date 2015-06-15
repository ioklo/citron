using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.IL.Commands
{
    // Branch
    public class Jump : ICommand
    {
        public int Block { get; private set; }

        public Jump(int block)
        {
            Block = block;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
