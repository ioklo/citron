using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.AbstractSyntax
{
    public class ForStmt : IStmt
    {
        public IStmt Initializer { get; set; }
        public IExp CondExp { get; set; }
        public IExp LoopExp { get; set; }
        public IStmt Body { get; set; }

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
