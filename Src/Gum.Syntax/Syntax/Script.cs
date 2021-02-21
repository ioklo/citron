using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gum.Syntax
{   
    // 가장 외곽
    public partial class Script : ISyntaxNode
    {
        public ImmutableArray<ScriptElement> Elements { get; }
        public Script(ImmutableArray<ScriptElement> elements)
        {
            Elements = elements;
        }        
    }
}