using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.StaticAnalysis
{
    public partial class Analyzer
    {
        public class LocalVarInfo
        {
            public string Name { get; }
            public TypeValue TypeValue { get; }

            public LocalVarInfo(string name, TypeValue typeValue)
            {
                Name = name;
                TypeValue = typeValue;
            }
        }
    }
}
