using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using Gum.Infra;

namespace Gum.Syntax
{
    public abstract record StructDeclElement : ISyntaxNode
    {
        // 외부에서 상속 금지
        internal StructDeclElement() { }
    }    

    public record TypeStructDeclElement(TypeDecl TypeDecl) : StructDeclElement;
    public record VarStructDeclElement(AccessModifier AccessModifier, TypeExp VarType, ImmutableArray<string> VarNames) : StructDeclElement;
    public record FuncStructDeclElement(StructFuncDecl FuncDecl) : StructDeclElement;
}
