using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.IL.Commands
{
    // Store(location, value) -> value
    public class Store : ICommand
    {
        public int DestRef { get; private set; }
        public int Src { get; private set; }

        public Store(int destRef, int src)
        {
            DestRef = destRef;
            Src = src;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
