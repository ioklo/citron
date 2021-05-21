﻿using System.Collections.Generic;
using Gum.Collections;
using Pretune;

namespace Gum.Syntax
{
    [AutoConstructor, ImplementIEquatable]
    public partial class GlobalFuncDecl : FuncDecl
    {
        public override bool IsSequence { get; } // seq 함수인가        
        public override TypeExp RetType { get; }
        public override string Name { get; }
        public override ImmutableArray<string> TypeParams { get; }
        public override FuncParamInfo ParamInfo { get; }
        public override BlockStmt Body { get; }
    }
}