using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.CompileTime
{
    // AppliedType
    public abstract record TypeId;

    // NormalType
    public record NormalTypeId : TypeId;
    
    public record RootTypeId(Name Module, NamespacePath? Namespace, Name Name, ImmutableArray<TypeId> TypeArgs) : NormalTypeId;
    public record MemberTypeId(NormalTypeId Outer, Name Name, ImmutableArray<TypeId> TypeArgs) : NormalTypeId;

    // 로컬, 사용한 곳의 환경에 따라 가리키는 것이 달라진다
    public sealed record TypeVarTypeId(int Index, string Name) : TypeId
    {
        public bool Equals(TypeVarTypeId? other)
        {
            if (other == null) return false;
            return Index == other.Index;
        }
        
        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }
    }   

    public record VoidTypeId : TypeId
    {
        public static readonly VoidTypeId Instance = new VoidTypeId();
        VoidTypeId() { }
    }

    // int?
    public record NullableTypeId(TypeId InnerType) : TypeId;
}
