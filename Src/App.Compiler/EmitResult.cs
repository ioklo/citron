using Gum.Core.IL.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.App.Compiler
{
    public class EmitResult
    {
        internal void Push(EmitResult condStmt)
        {
            throw new NotImplementedException();
        }

        internal void Push(ICommand cmd)
        {
            throw new NotImplementedException();
        }        

        internal void PushLabel(string p)
        {
            throw new NotImplementedException();
        }

        internal void PushIfNotJump(int condResult, string p)
        {
            throw new NotImplementedException();
        }

        internal void PushJump(string p)
        {
            throw new NotImplementedException();
        }
    }
}
