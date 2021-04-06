using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using System.Linq;
using Gum.Infra;

namespace Gum.Syntax
{
    public struct FuncParamInfo
    {
        public ImmutableArray<TypeAndName> Parameters { get; }
        public int? VariadicParamIndex { get; }

        public FuncParamInfo(ImmutableArray<TypeAndName> parameters, int? variadicParamIndex)
        {
            Parameters = parameters;
            VariadicParamIndex = variadicParamIndex;
        }
    }
}
