using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gum.Syntax
{
    // <RetTypeName> <FuncName> <LPAREN> <ARGS> <RPAREN>
    // LBRACE>
    // [Stmt]
    // <RBRACE>
    // a(b, params c, d);
    // a<T>(int b, params T x, int d);
    public class FuncDecl : ISyntaxNode
    {
        public FuncKind FuncKind { get; }
        public TypeExp RetType { get; }
        public string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<TypeAndName> Params { get; }
        public int? VariadicParamIndex { get; } 
        public BlockStmt Body { get; }

        public FuncDecl(
            FuncKind funcKind, 
            TypeExp retType, string name, ImmutableArray<string> typeParams, 
            ImmutableArray<TypeAndName> parameters, int? variadicParamIndex, BlockStmt body)
        {
            FuncKind = funcKind;
            RetType = retType;
            Name = name;
            TypeParams = typeParams;
            Params = parameters;
            VariadicParamIndex = variadicParamIndex;
            Body = body;
        }

        public FuncDecl(
            FuncKind funcKind, 
            TypeExp retType, string name, ImmutableArray<string> typeParams, 
            int? variadicParamIndex, BlockStmt body, params TypeAndName[] parameters)
        {
            FuncKind = funcKind;
            RetType = retType;
            Name = name;
            TypeParams = typeParams;
            Params = ImmutableArray.Create(parameters);
            VariadicParamIndex = variadicParamIndex;
            Body = body;
        }

        public override bool Equals(object? obj)
        {
            return obj is FuncDecl decl &&                   
                   FuncKind == decl.FuncKind &&
                   EqualityComparer<TypeExp>.Default.Equals(RetType, decl.RetType) &&
                   Name == decl.Name &&
                   Enumerable.SequenceEqual(TypeParams, decl.TypeParams) &&
                   Enumerable.SequenceEqual(Params, decl.Params) &&
                   VariadicParamIndex == decl.VariadicParamIndex &&
                   EqualityComparer<BlockStmt>.Default.Equals(Body, decl.Body);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RetType, Name, Params, VariadicParamIndex, Body);
        }

        public static bool operator ==(FuncDecl? left, FuncDecl? right)
        {
            return EqualityComparer<FuncDecl?>.Default.Equals(left, right);
        }

        public static bool operator !=(FuncDecl? left, FuncDecl? right)
        {
            return !(left == right);
        }
    }
}