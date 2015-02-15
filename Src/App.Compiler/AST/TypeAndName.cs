using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler.AST
{
    public class TypeAndName
    {
        public string TypeName { get; private set; }
        public string Name { get; private set; }

        public TypeAndName(string typeName, string name)
        {
            TypeName = typeName;
            Name = name;
        }
    }
}
