using System;
using System.Collections.Generic;

namespace Gum.Syntax
{
    public abstract class ScriptElement
    {
        internal ScriptElement() { }
    }

    public class GlobalFuncDeclScriptElement : ScriptElement
    {
        public GlobalFuncDecl FuncDecl { get; }
        public GlobalFuncDeclScriptElement(GlobalFuncDecl funcDecl)
        {
            FuncDecl = funcDecl;
        }
    }

    public class StmtScriptElement : ScriptElement
    {
        public Stmt Stmt { get; }
        public StmtScriptElement(Stmt stmt)
        {
            Stmt = stmt;
        }
    }

    public class TypeDeclScriptElement : ScriptElement
    {
        public TypeDecl TypeDecl { get; }
        public TypeDeclScriptElement(TypeDecl typeDecl) { TypeDecl = typeDecl; }
    }
}