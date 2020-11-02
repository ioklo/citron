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
    }
}