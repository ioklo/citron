using Pretune;
using System;
using System.Collections.Generic;
using System.Text;
using M = Gum.CompileTime;

namespace Gum.CompileTime
{
    [ImplementIEquatable]
    public partial struct ItemPathEntry
    {
        public M.Name Name { get; }
        public int TypeParamCount { get; }
        public M.ParamTypes ParamTypes { get; }   // 함수에서 파라미터에 따라 달라지는 값

        public ItemPathEntry(M.Name name, int typeParamCount = 0, M.ParamTypes paramTypes = default)
        {
            Name = name;
            TypeParamCount = typeParamCount;
            ParamTypes = paramTypes;
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