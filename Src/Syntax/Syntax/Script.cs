using Citron.Syntax;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using Pretune;

namespace Citron.Syntax
{   
    // 가장 외곽
    public record Script(ImmutableArray<ScriptElement> Elements) : ISyntaxNode;
}