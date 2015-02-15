using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler.AST
{
    public class VarDecl : IDecl
    {
        public string TypeName { get; private set; }
        public List<NameAndExp> NameAndExps { get; private set; }

        public VarDecl(string typeName)
        {
            TypeName = typeName;
            NameAndExps = new List<NameAndExp>();
        }
    }
}
