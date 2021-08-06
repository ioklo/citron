﻿using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using Gum.Infra;

namespace Gum.Syntax
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
        ImmutableArray<string> TypeParams,
        ImmutableArray<FuncParam> Parameters,
        BlockStmt Body
    ) : StructMemberDecl;
    public record StructMemberVarDecl(AccessModifier? AccessModifier, TypeExp VarType, ImmutableArray<string> VarNames) : StructMemberDecl;

    public record StructConstructorDecl(
        AccessModifier? AccessModifier,
        string Name,
        ImmutableArray<FuncParam> Parameters,
        BlockStmt Body) : StructMemberDecl;
}
