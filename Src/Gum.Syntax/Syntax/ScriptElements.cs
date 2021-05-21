using Pretune;
using System;
using System.Collections.Generic;

namespace Gum.Syntax
{
    public abstract record ScriptElement
    {
        internal ScriptElement() { }
    }
    
    public record GlobalFuncDeclScriptElement(GlobalFuncDecl FuncDecl) : ScriptElement;    
    public record StmtScriptElement(Stmt Stmt) : ScriptElement;
    public record TypeDeclScriptElement(TypeDecl TypeDecl) : ScriptElement;
}