using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Gum.CompileTime
{
    public class FuncInfo : ItemInfo, IEquatable<FuncInfo?>
    {
        public override Name Name { get; }
        public bool IsSequenceFunc { get; }
        public bool IsInstanceFunc { get; }
        public ImmutableArray<string> TypeParams { get; }
        public Type RetType { get; }
        public ImmutableArray<Type> ParamTypes { get; }

        public FuncInfo(Name name, bool bSeqCall, bool bThisCall, ImmutableArray<string> typeParams, Type retType, ImmutableArray<Type> paramTypes)
        {
            Name = name;
            IsSequenceFunc = bSeqCall;
            IsInstanceFunc = bThisCall;
            TypeParams = typeParams;
            RetType = retType;
            ParamTypes = paramTypes;
        }

        public FuncInfo(Name name, bool bSeqCall, bool bThisCall, IEnumerable<string> typeParams, Type retType, IEnumerable<Type> paramTypes)
            : this(name, bSeqCall, bThisCall, typeParams.ToImmutableArray(), retType, paramTypes.ToImmutableArray()) { }

        public FuncInfo(Name name, bool bSeqCall, bool bThisCall, ImmutableArray<string> typeParams, Type retType, params Type[] paramTypes)
            : this(name, bSeqCall, bThisCall, typeParams, retType, paramTypes.ToImmutableArray()) { }

        public override bool Equals(object? obj)
        {
            return Equals(obj as FuncInfo);
        }

        public bool Equals(FuncInfo? other)
        {
            return other != null &&
                   Name.Equals(other.Name) &&
                   Name.Equals(other.Name) &&
                   IsSequenceFunc == other.IsSequenceFunc &&
                   IsInstanceFunc == other.IsInstanceFunc &&
                   TypeParams.SequenceEqual(other.TypeParams) &&
                   EqualityComparer<Type>.Default.Equals(RetType, other.RetType) &&
                   ParamTypes.SequenceEqual(other.ParamTypes);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Name, IsSequenceFunc, IsInstanceFunc, TypeParams, RetType, ParamTypes);
        }
    }
}
