using Citron.Infra;
using Pretune;

namespace Citron.Symbol
{
    public record struct FuncReturnId(TypeId TypeId) : ISerializable
    {   
        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(TypeId), TypeId);
        }
    }

    public record struct FuncReturn(IType Type) : ICyclicEqualityComparableStruct<FuncReturn>, ISerializable
    {
        public FuncReturnId GetId()
        {
            return new FuncReturnId(Type.GetTypeId());
        }

        public FuncReturn Apply(TypeEnv typeEnv)
        {
            var appliedType = Type.Apply(typeEnv);
            return new FuncReturn(appliedType);
        }

        bool ICyclicEqualityComparableStruct<FuncReturn>.CyclicEquals(ref FuncReturn other, ref CyclicEqualityCompareContext context)
        {   
            if (!context.CompareClass(Type, other.Type))
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {   
            context.SerializeRef(nameof(Type), Type);
        }
    }
}