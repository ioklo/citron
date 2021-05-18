using Gum.Syntax;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using Pretune;

namespace Gum.Syntax
{   
    // 가장 외곽
    [AutoConstructor, ImplementIEquatable]
    public partial class Script : ISyntaxNode
    {
        public ImmutableArray<ScriptElement> Elements { get; }
    }
}