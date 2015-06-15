using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite.Generator
{
    class StructVar
    {
        public VarType VarType { get; private set; }
        public BaseElement Type { get; private set; }
        public string Name { get; private set; }

        public StructVar(VarType varType, BaseElement type, string name)
        {
            VarType = varType;
            Type = type;
            Name = name;
        }
    }
}
