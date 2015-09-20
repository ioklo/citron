using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Test.Type
{
    class FuncTypeVar : IType
    {
        public int Index { get; private set; }

        public FuncTypeVar(int index) { Index = index; }

        public IEnumerable<IType> GetMemberTypes(Environment env, string memberName)
        {
            throw new NotImplementedException();
        }
    }
}
