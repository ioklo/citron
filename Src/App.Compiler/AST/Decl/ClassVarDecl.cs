using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.App.Compiler.AST
{
    public class ClassVarDecl
    {
        public Lexer.TokenKind AccessorKind { get; private set; }
        public VarDecl VarDecl { get; private set; }

        public ClassVarDecl(Lexer.TokenKind accessorKind, AST.VarDecl varDecl)
        {
            this.AccessorKind = accessorKind;
            this.VarDecl = varDecl;
        }
    }
}
