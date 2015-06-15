using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL.Commands
{
    public class VirtualCall : ICommand
    {
        public int Func { get; private set; }
        public IReadOnlyList<int> Args{ get; private set; }

        public VirtualCall(int func, IEnumerable<int> args)
        {
            Func = func;
            Args = args.ToList();
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
