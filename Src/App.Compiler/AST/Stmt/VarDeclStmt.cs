using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler.AST
{
    public class VarDeclStmt : IStmt
    {
        public VarDecl Decl { get; private set; }

        public VarDeclStmt(VarDecl decl)
        {
            Decl = decl;
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
