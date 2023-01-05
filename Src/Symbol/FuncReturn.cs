using Citron.Infra;
using Pretune;

namespace Citron.Symbol
{       
    public record struct FuncReturn(bool IsRef, IType Type) : ICyclicEqualityComparableStruct<FuncReturn>
    {
        public FuncReturn Apply(TypeEnv typeEnv)
        {
            var appliedType = Type.Apply(typeEnv);
            return new FuncReturn(IsRef, appliedType);
        }

        bool ICyclicEqualityComparableStruct<FuncReturn>.CyclicEquals(ref FuncReturn other, ref CyclicEqualityCompareContext context)
        {
            if (!IsRef.Equals(other.IsRef))
                return false;

            if (!context.CompareClass(Type, other.Type))
                return false;

            return true;
        }
    }
}