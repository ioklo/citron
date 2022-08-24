using Pretune;
using System;
using System.Collections.Generic;

namespace Citron.Syntax
{
    public abstract record ScriptElement
    {
        internal ScriptElement() { }
    }

    public record NamespaceDeclScriptElement(NamespaceDecl NamespaceDecl): ScriptElement;    
    public record GlobalFuncDeclScriptElement(GlobalFuncDecl FuncDecl) : ScriptElement;    
    public record StmtScriptElement(Stmt Stmt) : ScriptElement;
    public record TypeDeclScriptElement(TypeDecl TypeDecl) : ScriptElement;
}