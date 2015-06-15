using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.AbstractSyntax
{
    public class BlockStmt : IStmt
    {
        public IReadOnlyList<IStmt> Stmts { get; private set; }

        public BlockStmt(IEnumerable<IStmt> stmts)
        {
            Stmts = new List<IStmt>(stmts);
        }

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
