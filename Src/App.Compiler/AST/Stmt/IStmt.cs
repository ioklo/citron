using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler.AST
{
    public interface IStmt
    {
        void Visit(IStmtVisitor visitor);
        Result Visit<Result>(IStmtVisitor<Result> visitor);
    }
}
