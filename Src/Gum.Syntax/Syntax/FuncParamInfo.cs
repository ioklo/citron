using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Linq;
using Gum.Infra;

namespace Gum.Syntax
{
    public struct FuncParamInfo
    {
        public ImmutableArray<TypeAndName> Parameters { get; }
        public int? VariadicParamIndex { get; }

        public FuncParamInfo(IEnumerable<TypeAndName> parameters, int? variadicParamIndex)
        {
            Parameters = parameters.ToImmutableArray();
            VariadicParamIndex = variadicParamIndex;
        }
    }
}
