using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL
{
    // type information
    public class TypeInfo
    {
        // BaseType 
        // public TypeInfo Base { get; private set; }

        // public List<FieldInfo> Fields { get; private set; }
        // private Dictionary<string, FuncInfo> funcs = new Dictionary<string, FuncInfo>();

        public int FieldCount { get; private set; }
        public string Name { get; private set; }

        public TypeInfo(string name, int fieldCount)
        {
            Name = name;
            FieldCount = fieldCount;
        }
    }
}
