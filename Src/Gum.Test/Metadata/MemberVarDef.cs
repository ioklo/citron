using Gum.Test.TypeInst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Test.Metadata
{
    class MemberVarDef
    {
        public TypeDef MemberOf {get; private set;}
        public ITypeInst TypeInst {get; private set;}
        public string Name { get; private set; }

        public MemberVarDef(TypeDef memberOf, ITypeInst typeInst, string name)            
        {
            MemberOf = memberOf;
            TypeInst = typeInst;
        }
    }
}
