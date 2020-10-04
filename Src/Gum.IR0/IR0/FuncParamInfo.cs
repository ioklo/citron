using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Linq;
using Gum.Infra;

namespace Gum.IR0
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

        public override bool Equals(object? obj)
        {
            return obj is FuncParamInfo info &&
                   Enumerable.SequenceEqual(Parameters, info.Parameters) &&
                   VariadicParamIndex == info.VariadicParamIndex;
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            SeqEqComparer.AddHash(ref hashCode, Parameters);

            return HashCode.Combine(Parameters, VariadicParamIndex);
        }

        public static bool operator ==(FuncParamInfo left, FuncParamInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FuncParamInfo left, FuncParamInfo right)
        {
            return !(left == right);
        }
    }
}
