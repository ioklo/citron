using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;

using S = Gum.Syntax;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {   
        ImmutableArray<TypeValue> GetTypeValues(ImmutableArray<S.TypeExp> typeExps)
        {
            return typeExps.Select(typeExp => context.GetTypeValueByTypeExp(typeExp)).ToImmutableArray();
        }        
    }
}
