using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler
{
    // During Compile time type information
    public class TypeInfo
    {
        // field information 
        public string Name {get; private set;}
        public Dictionary<string, FieldInfo> Fields { get; private set; }

        public TypeInfo(string name)
        {
            Name = name;
            Fields = new Dictionary<string, FieldInfo>();
        }

        public void AddField(AccessModifier accessor, TypeInfo typeInfo, string name)
        {
            Fields.Add(name, new FieldInfo(accessor, typeInfo, name, Fields.Count));
        }
    }
}
