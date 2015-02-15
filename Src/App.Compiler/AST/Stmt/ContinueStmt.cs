using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler.AST
{
    public class ContinueStmt : IStmt
    {
        public void Visit(IStmtVisitor visitor)
        {
            visitor.Visit(this);
        }

        public Result Visit<Result>(IStmtVisitor<Result> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
