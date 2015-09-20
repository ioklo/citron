using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Test.Type
{
    class ObjectTypeVar : IType
    {
        public int Index { get; private set; }

        public ObjectTypeVar(int index) { Index = index; }

        public IEnumerable<IType> GetMemberTypes(Environment env, string memberName)
        {
            throw new NotImplementedException();
        }
    }
}
