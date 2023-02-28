using Pretune;
using System;
using System.Collections.Generic;

namespace Citron.Syntax
{
    public abstract record class ScriptElement
    {
        internal ScriptElement() { }
    }

    public record class NamespaceDeclScriptElement(NamespaceDecl NamespaceDecl): ScriptElement;    
    public record class GlobalFuncDeclScriptElement(GlobalFuncDecl FuncDecl) : ScriptElement;
    public record class TypeDeclScriptElement(TypeDecl TypeDecl) : ScriptElement;
}