using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.IL.Commands
{
    // Static call/non-virtual call
    public class StaticCall : ICommand
    {
        public int Ret { get; private set;  }
        public int Func { get; private set; }
        public IReadOnlyList<int> Args{ get; private set; }

        public StaticCall(int ret, int func, IEnumerable<int> args)
        {
            Ret = ret;
            Func = func;
            Args = args.ToList();
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
