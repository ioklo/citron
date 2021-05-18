using Pretune;
using System;
using System.Collections.Generic;

namespace Gum.Syntax
{
    public abstract class ScriptElement
    {
        internal ScriptElement() { }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class GlobalFuncDeclScriptElement : ScriptElement
    {
        public GlobalFuncDecl FuncDecl { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class StmtScriptElement : ScriptElement
    {
        public Stmt Stmt { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class TypeDeclScriptElement : ScriptElement
    {
        public TypeDecl TypeDecl { get; }
    }
}