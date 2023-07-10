using System.Collections.Generic;
using Citron.Collections;
using Pretune;

namespace Citron.Syntax
{
    [AutoConstructor, ImplementIEquatable]
    public partial class GlobalFuncDecl : ISyntaxNode
    {
        public AccessModifier? AccessModifier { get; }
        public bool IsSequence { get; } // seq 함수인가        
        public TypeExp RetType { get; }
        public string Name { get; }
        public ImmutableArray<TypeParam> TypeParams { get; }
        public ImmutableArray<FuncParam> Parameters { get; }
        public bool IsVariadic { get; }
        public ImmutableArray<Stmt> Body { get; }
    }
}