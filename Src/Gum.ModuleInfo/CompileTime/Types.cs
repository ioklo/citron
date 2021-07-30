using Gum.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.CompileTime
{
    // AppliedType
    public abstract record Type;
    public abstract record NormalType : Type;

    public record GlobalType(ModuleName ModuleName, NamespacePath NamespacePath, Name Name, ImmutableArray<Type> TypeArgs) : NormalType;
    public record MemberType(NormalType Outer, Name Name, ImmutableArray<Type> TypeArgs) : NormalType;

    // 로컬, 사용한 곳의 환경에 따라 가리키는 것이 달라진다
    public sealed record TypeVarType(int Index, string Name) : Type
    {
        public bool Equals(TypeVarType? other)
        {
            if (other == null) return false;
            return Index == other.Index;
        }
        
        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }
    }   

    public record VoidType : Type
    {
        public static readonly VoidType Instance = new VoidType();
        VoidType() { }
    }
}
