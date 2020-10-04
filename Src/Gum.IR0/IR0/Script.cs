using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.IR0
{
    public partial class Script
    {
        public int PrivateGlobalVarCount { get; }
        public int LocalVarCount { get; }

        public Script(int privateGlobalVarCount, int localVarCount)
        {
            PrivateGlobalVarCount = privateGlobalVarCount;
            LocalVarCount = localVarCount;
        }
    }
}
