using Gum.Infra;
using Gum.Syntax;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;

namespace Gum.Syntax
{
    // <RetTypeName> <FuncName> <LPAREN> <ARGS> <RPAREN>
    // LBRACE>
    // [Stmt]
    // <RBRACE>
    // a(b, params c, d);
    // a<T>(int b, params T x, int d);
    public abstract class FuncDecl : ISyntaxNode
    {
        public abstract bool IsSequence { get; } // seq 함수인가        
        public abstract TypeExp RetType { get; }
        public abstract string Name { get; }
        public abstract ImmutableArray<string> TypeParams { get; }
        public abstract FuncParamInfo ParamInfo { get; }
        public abstract BlockStmt Body { get; }
    }
}