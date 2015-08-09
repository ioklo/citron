using Gum.Test.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Test.TypeInst
{
    class TypeWithVar : ITypeInst
    {
        public TypeDef TypeDef { get; private set; }
        public IReadOnlyList<ITypeInst> TypeVars { get; private set; }

        public TypeWithVar(TypeDef typeDef, IEnumerable<ITypeInst> typeVars)
        {
            TypeDef = typeDef;
            TypeVars = typeVars.ToList();
        }

        public IEnumerable<ITypeInst> GetMemberTypes(Environment env, string memberName)
        {
            throw new NotImplementedException();
        }
    }
}
