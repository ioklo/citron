using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.AbstractSyntax
{
    public interface IStmtVisitor
    {
        void Visit(BlockStmt stmt);
        void Visit(VarDecl stmt);
        void Visit(ForStmt stmt);
        void Visit(WhileStmt stmt);
        void Visit(DoWhileStmt stmt);
        void Visit(IfStmt stmt);
        void Visit(ExpStmt stmt);
        void Visit(ReturnStmt stmt);
        void Visit(ContinueStmt stmt);
        void Visit(BreakStmt stmt);
    }

    public interface IStmtVisitor<Result>
    {
        Result Visit(BlockStmt stmt);
        Result Visit(VarDecl stmt);
        Result Visit(ForStmt stmt);
        Result Visit(WhileStmt stmt);
        Result Visit(DoWhileStmt stmt);
        Result Visit(IfStmt stmt);        
        Result Visit(ExpStmt stmt);
        Result Visit(ReturnStmt stmt);
        Result Visit(ContinueStmt stmt);
        Result Visit(BreakStmt stmt);
    }
}
