using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL.Commands
{
    public class Load : ICommand
    {
        public int Dest { get; private set; }
        public int SrcRef { get; private set; }

        public Load(int dest, int srcRef )
        {
            Dest = dest;
            SrcRef = srcRef;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
