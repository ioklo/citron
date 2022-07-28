﻿using Citron.Infra;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Pretune;

namespace Citron.Syntax
{
    public abstract record Exp : ISyntaxNode;
    public record IdentifierExp(string Value, ImmutableArray<TypeExp> TypeArgs) : Exp;    
    public record StringExp(ImmutableArray<StringExpElement> Elements) : Exp;    
    public record IntLiteralExp(int Value) : Exp;
    public record BoolLiteralExp(bool Value) : Exp;
    // null
    public record NullLiteralExp : Exp
    {
        public static readonly NullLiteralExp Instance = new NullLiteralExp();
        NullLiteralExp() { }
    }

    public record BinaryOpExp(BinaryOpKind Kind, Exp Operand0, Exp Operand1) : Exp;
    public record UnaryOpExp(UnaryOpKind Kind, Exp Operand) : Exp;
    public record CallExp(Exp Callable, ImmutableArray<Argument> Args) : Exp;

    [AutoConstructor, ImplementIEquatable]
    public partial struct LambdaExpParam
    {
        public FuncParamKind ParamKind { get; }
        public TypeExp? Type { get; }           // 타입은 생략 가능
        public string Name { get; }
    }

    public record LambdaExp(ImmutableArray<LambdaExpParam> Params, ImmutableArray<Stmt> Body) : Exp;
    
    // a[b]
    public record IndexerExp(Exp Object, Exp Index) : Exp;    
    public record MemberExp(Exp Parent, string MemberName, ImmutableArray<TypeExp> MemberTypeArgs) : Exp;
    public record ListExp(TypeExp? ElemType, ImmutableArray<Exp> Elems) : Exp;

    // new Type(2, 3, 4);
    public record NewExp(TypeExp Type, ImmutableArray<Argument> Args) : Exp;
}