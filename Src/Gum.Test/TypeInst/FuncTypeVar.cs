using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Test.TypeInst
{
    class FuncTypeVar : ITypeInst
    {
        public int Index { get; private set; }

        public FuncTypeVar(int index) { Index = index; }

        public IEnumerable<ITypeInst> GetMemberTypes(Environment env, string memberName)
        {
            throw new NotImplementedException();
        }
    }
}
