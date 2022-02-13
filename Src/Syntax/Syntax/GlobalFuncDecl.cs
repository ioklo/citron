using System.Collections.Generic;
using Citron.Collections;
using Pretune;

namespace Citron.Syntax
{
    [AutoConstructor, ImplementIEquatable]
    public partial class GlobalFuncDecl
    {
        public AccessModifier? AccessModifier { get; }
        public bool IsSequence { get; } // seq 함수인가        
        public bool IsRefReturn { get; }
        public TypeExp RetType { get; }
        public string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<FuncParam> Parameters { get; }
        public BlockStmt Body { get; }
    }
}