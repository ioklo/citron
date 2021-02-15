using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

using M = Gum.CompileTime;

namespace Gum.IR0
{       
    class TypeEnv
    {
        internal struct DepthIndex : IEquatable<DepthIndex>
        {
            public int Depth { get; }
            public int Index { get; }
            public DepthIndex(int depth, int index) { Depth = depth; Index = index; }

            public override bool Equals(object? obj)
            {
                return obj is DepthIndex index && Equals(index);
            }

            public bool Equals(DepthIndex other)
            {
                return Depth == other.Depth &&
                       Index == other.Index;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Depth, Index);
            }
        }

        ImmutableDictionary<DepthIndex, TypeValue> dict;
        
        public TypeEnv(ImmutableDictionary<DepthIndex, TypeValue> dict)
        {
            this.dict = dict;
        }

        public TypeValue GetValue(int depth, int index)
        {
            return dict[new DepthIndex(depth, index)];
        }
    }
}