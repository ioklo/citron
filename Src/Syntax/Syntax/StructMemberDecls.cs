using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Text;
using Citron.Infra;

namespace Citron.Syntax
{
    public abstract record class StructMemberDecl : ISyntaxNode
    {
        // 외부에서 상속 금지
        internal StructMemberDecl() { }
    }    

    public record class StructMemberTypeDecl(TypeDecl TypeDecl) : StructMemberDecl;
    public record class StructMemberFuncDecl(
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
    public record class StructMemberVarDecl(AccessModifier? AccessModifier, TypeExp VarType, ImmutableArray<string> VarNames) : StructMemberDecl;

    public record class StructConstructorDecl(
        AccessModifier? AccessModifier,
        string Name,
        ImmutableArray<FuncParam> Parameters,
        ImmutableArray<Stmt> Body) : StructMemberDecl;
}
