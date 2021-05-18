using Gum.Syntax;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using Pretune;

namespace Gum.Syntax
{   
    // 가장 외곽
    public record Script(ImmutableArray<ScriptElement> Elements) : ISyntaxNode;
}