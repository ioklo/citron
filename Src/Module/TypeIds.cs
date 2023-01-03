using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Module
{
    // AppliedType
    public abstract record class TypeId;

    // NormalType
    public record class NormalTypeId : TypeId;
    
    public record class RootTypeId(Name Module, NamespacePath? Namespace, Name Name, ImmutableArray<TypeId> TypeArgs) : NormalTypeId;
    public record class MemberTypeId(NormalTypeId Outer, Name Name, ImmutableArray<TypeId> TypeArgs) : NormalTypeId;
     
    // 로컬, 사용한 곳의 환경에 따라 가리키는 것이 달라진다 (중첩시 누적 Index를 쓰도록 한다)
    public sealed record class TypeVarTypeId(int Index, string Name) : TypeId
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

    public record class VoidTypeId : TypeId
    {
        public static readonly VoidTypeId Instance = new VoidTypeId();
        VoidTypeId() { }
    }

    // int?
    public record class NullableTypeId(TypeId InnerType) : TypeId;
}
