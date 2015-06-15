using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.IL.Commands
{
    // class/struct support
    public class New : ICommand
    {
        public int DestReg { get; private set; }
        public int Type { get; private set; }
        public IReadOnlyList<int> TypeArgs { get; private set; } // for Generic Support

        public New(int destReg, int type, IEnumerable<int> typeArgs )
        {
            DestReg = destReg;
            Type = type;
            TypeArgs = typeArgs.ToList();
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
