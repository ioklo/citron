using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Metadatas
{
    class MemberVarDef
    {
        public TypeDef MemberOf {get; private set;}
        public IType Type {get; private set;}
        public string Name { get; private set; }

        public MemberVarDef(TypeDef memberOf, IType type, string name)            
        {
            MemberOf = memberOf;
            Type = type;
        }
    }
}
