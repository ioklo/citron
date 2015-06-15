using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.AbstractSyntax
{
    public interface IFuncStmtVisitor
    {
        void Visit(VarDecl stmt);
        void Visit(BlockStmt stmt);        
        void Visit(ForStmt stmt);
        void Visit(WhileStmt stmt);
        void Visit(DoWhileStmt stmt);
        void Visit(IfStmt stmt);
        void Visit(ExpStmt stmt);
        void Visit(ReturnStmt stmt);
    }
}
