using System;
using System.Collections.Generic;

namespace Gum.Syntax
{
    public partial class Script
    {
        public abstract class Element
        {
            internal Element() { }
        }

        public class FuncDeclElement : Element
        {
            public FuncDecl FuncDecl { get; }
            public FuncDeclElement(FuncDecl funcDecl)
            {
                FuncDecl = funcDecl;
            }
        }

        public class StmtElement : Element
        {
            public Stmt Stmt { get; }
            public StmtElement(Stmt stmt)
            {
                Stmt = stmt;
            }            
        }

        public class TypeDeclElement : Element
        {
            public TypeDecl TypeDecl { get; }
            public TypeDeclElement(TypeDecl typeDecl ) { TypeDecl = typeDecl; }
        }
    }
}