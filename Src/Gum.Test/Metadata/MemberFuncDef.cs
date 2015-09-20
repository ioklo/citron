using Gum.Test.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Test.Metadata
{
    class MemberFuncDef
    {
        public TypeDef MemberOf { get; private set; }
        public int TypeParamCount { get; private set; } // Variadic 이 가능해야 한다.

        public IType ReturnType { get; private set; }
        public IReadOnlyList<IType> ParamTypes { get; private set; }

        public MemberFuncDef(TypeDef memberOf, string name, int typeParamCount, IType returnType, IEnumerable<IType> paramTypes)
        {
            MemberOf = memberOf;

            TypeParamCount = typeParamCount;
            ReturnType = returnType;
            ParamTypes = paramTypes.ToList();
        }
    }
}
