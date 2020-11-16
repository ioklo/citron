using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.CompileTime
{
    public struct ItemPathEntry
    {
        public Name Name { get; }
        public int TypeParamCount { get; }
        public string ParamHash { get; }   // 함수에서 파라미터에 따라 달라지는 값

        public ItemPathEntry(Name name, int typeParamCount = 0, string paramHash = "")
        {
            Name = name;
            TypeParamCount = typeParamCount;
            ParamHash = paramHash;
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