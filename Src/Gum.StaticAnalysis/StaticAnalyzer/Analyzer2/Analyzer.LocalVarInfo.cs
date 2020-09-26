using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.StaticAnalysis
{
    public partial class Analyzer2
    {
        public class LocalVarInfo
        {
            public int Index { get; }
            public TypeValue TypeValue { get; }

            public LocalVarInfo(int index, TypeValue typeValue)
            {
                Index = index;
                TypeValue = typeValue;
            }
        }
    }
}
