using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler.AST
{
    public class NewExp : IExp
    {
        public string TypeName { get; set; }
        public List<IExp> Args { get; private set; }

        public NewExp(string typeName)
        {
            TypeName = typeName;
            Args = new List<IExp>();
        }

        public void Visit(IExpVisitor visitor)
        {
            visitor.Visit(this);
        }

        public Result Visit<Result>(IExpVisitor<Result> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
