using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Test.TypeInst
{
    class ObjectTypeVar : ITypeInst
    {
        public int Index { get; private set; }

        public ObjectTypeVar(int index) { Index = index; }

        public IEnumerable<ITypeInst> GetMemberTypes(Environment env, string memberName)
        {
            throw new NotImplementedException();
        }
    }
}
