using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler.AST
{
    public class BlockStmt : IStmt
    {
        public List<IStmt> Stmts { get; private set; }

        public BlockStmt()
        {
            Stmts = new List<IStmt>();
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
