using Gum.Test.TypeInst;
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

        public ITypeInst ReturnType { get; private set; }
        public IReadOnlyList<ITypeInst> ParamTypes { get; private set; }

        public MemberFuncDef(TypeDef memberOf, string name, int typeParamCount, ITypeInst returnType, IEnumerable<ITypeInst> paramTypes)
        {
            MemberOf = memberOf;

            TypeParamCount = typeParamCount;
            ReturnType = returnType;
            ParamTypes = paramTypes.ToList();
        }
    }
}
