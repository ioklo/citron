using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler.AST
{
    public class AssignExp : IExp
    {
        public VariableExp Var { get; set; }
        public IExp Exp { get; set; }

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
