using Gum.Infra;
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
        public bool IsSequence { get; } // seq 함수인가        
        public TypeExp RetType { get; }
        public string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public FuncParamInfo ParamInfo { get; }
        public BlockStmt Body { get; }

        public FuncDecl(
            bool bSequence,
            TypeExp retType, string name, IEnumerable<string> typeParams, 
            FuncParamInfo paramInfo, BlockStmt body)
        {
            IsSequence = bSequence;
            RetType = retType;
            Name = name;
            TypeParams = typeParams.ToImmutableArray();
            ParamInfo = paramInfo;
            Body = body;
        }
        
        public override bool Equals(object? obj)
        {
            return obj is FuncDecl decl &&                   
                   IsSequence == decl.IsSequence &&
                   EqualityComparer<TypeExp>.Default.Equals(RetType, decl.RetType) &&
                   Name == decl.Name &&
                   Enumerable.SequenceEqual(TypeParams, decl.TypeParams) &&
                   EqualityComparer<FuncParamInfo>.Default.Equals(ParamInfo, decl.ParamInfo) &&                   
                   EqualityComparer<BlockStmt>.Default.Equals(Body, decl.Body);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(IsSequence);
            hashCode.Add(RetType);
            hashCode.Add(Name);
            SeqEqComparer.AddHash(ref hashCode, TypeParams);
            hashCode.Add(ParamInfo);
            hashCode.Add(Body);

            return hashCode.ToHashCode();
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