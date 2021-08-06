using System.Collections.Generic;
using Gum.Collections;
using Pretune;

namespace Gum.Syntax
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