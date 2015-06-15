using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.AbstractSyntax
{
    public class ExpStmt : IStmt, IREPLStmt
    {
        public IExp Exp { get; set; }

        public void Visit(IStmtVisitor visitor)
        {
            visitor.Visit(this);
        }

        public Result Visit<Result>(IStmtVisitor<Result> visitor)
        {
            return visitor.Visit(this);
        }

        public void Visit(IREPLStmtVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

}
