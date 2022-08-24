using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Text;
using Citron.Infra;

namespace Citron.Syntax
{
    public abstract record StructMemberDecl : ISyntaxNode
    {
        // 외부에서 상속 금지
        internal StructMemberDecl() { }
    }    

    public record StructMemberTypeDecl(TypeDecl TypeDecl) : StructMemberDecl;
    public record StructMemberFuncDecl(
        AccessModifier? AccessModifier,
        bool IsStatic,
        bool IsSequence, // seq 함수인가        
        bool IsRefReturn,
        TypeExp RetType,
        string Name,
        ImmutableArray<TypeParam> TypeParams,
        ImmutableArray<FuncParam> Parameters,
        ImmutableArray<Stmt> Body
    ) : StructMemberDecl;
    public record StructMemberVarDecl(AccessModifier? AccessModifier, TypeExp VarType, ImmutableArray<string> VarNames) : StructMemberDecl;

    public record StructConstructorDecl(
        AccessModifier? AccessModifier,
        string Name,
        ImmutableArray<FuncParam> Parameters,
        ImmutableArray<Stmt> Body) : StructMemberDecl;
}
