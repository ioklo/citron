using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL.Commands
{
    // 현재 Location
    public class FieldRef : ICommand
    {
        public int DestReg { get; private set; }
        public int SrcRefReg { get; private set; }
        public int Index { get; private set; }

        public FieldRef(int dest, int srcRefReg, int index)
        {
            DestReg = dest;
            SrcRefReg = srcRefReg;
            Index = index;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
