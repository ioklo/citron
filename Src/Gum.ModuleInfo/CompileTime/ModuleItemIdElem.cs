using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.CompileTime
{
    public struct ModuleItemIdElem
    {
        public Name Name { get; }
        public int TypeParamCount { get; }
        
        public ModuleItemIdElem(Name name, int typeParamCount = 0)
        {
            Name = name;
            TypeParamCount = typeParamCount;
        }

        public ModuleItemIdElem(string name, int typeParamCount = 0)
        {
            Name = Name.MakeText(name);
            TypeParamCount = typeParamCount;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);            
            return sb.ToString();
        }

        public void ToString(StringBuilder sb)
        {
            sb.Append(Name);

            if (TypeParamCount != 0)
            {
                sb.Append('<');
                for (int i = 0; i < TypeParamCount - 1; i++)
                    sb.Append(',');
                sb.Append('>');
            }
        }
    }
}