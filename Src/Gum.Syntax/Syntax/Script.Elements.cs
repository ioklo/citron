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

        public class EnumDeclElement : Element
        {
            public EnumDecl EnumDecl { get; }
            public EnumDeclElement(EnumDecl enumDecl) { EnumDecl = enumDecl; }
        }

        public class StructDeclElement : Element
        {
            public StructDecl StructDecl { get; }
            public StructDeclElement(StructDecl structDecl) { StructDecl = structDecl; }
        }
    }
}