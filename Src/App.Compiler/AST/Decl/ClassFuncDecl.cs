using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.App.Compiler.AST
{
    public class ClassFuncDecl
    {
        public Lexer.TokenKind AccessorKind { get; private set; }
        public Lexer.TokenKind InheritKind { get; private set; }
        public FuncDecl FuncDecl { get; private set; }

        public ClassFuncDecl(Lexer.TokenKind accessorKind, Lexer.TokenKind inheritKind, AST.FuncDecl funcDecl)
        {
            // TODO: Complete member initialization
            this.AccessorKind = accessorKind;
            this.InheritKind = inheritKind;
            this.FuncDecl = funcDecl;
        }

    }
}
