using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gum.IR0
{
    // TypeDeclId와 TypeContext를 묶어서 Type이라고 했으므로
    // 보조를 맞추기 위해 FuncDeclId와 TypeContext를 묶어서 Func라 한다
    public struct Func
    {
        public FuncDeclId DeclId { get; }
        public TypeContext TypeContext { get; }

        public Func(FuncDeclId declId, TypeContext typeContext)
        {
            DeclId = declId;
            TypeContext = typeContext;
        }
    }
}