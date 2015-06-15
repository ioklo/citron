using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.AbstractSyntax
{
    public interface IREPLStmtVisitor
    {
        // void Visit(ClassDecl classDecl);
        void Visit(FuncDecl funcDecl);
        void Visit(VarDecl varDecl);
        void Visit(ExpStmt expStmt);
    }
}
