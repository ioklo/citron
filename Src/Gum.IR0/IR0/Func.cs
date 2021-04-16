using Gum.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;

namespace Gum.IR0
{
    // TypeDeclId와 TypeContext를 묶어서 Type이라고 했으므로
    // 보조를 맞추기 위해 FuncDeclId와 TypeContext를 묶어서 Func라 한다
    [AutoConstructor, ImplementIEquatable]
    public partial struct Func
    {
        public DeclId DeclId { get; }
        public TypeContext TypeContext { get; }
    }
}