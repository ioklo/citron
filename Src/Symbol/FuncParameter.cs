using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;

namespace Citron.Symbol
{
    public record struct FuncParameterId(IType Type) : ISerializable
    {
        void ISerializable.DoSerialize(ref SerializeContext context)
        {   
            context.SerializeRef(nameof(Type), Type);
        }
    }

    // value
    public record struct FuncParameter(IType Type, Name Name) : ICyclicEqualityComparableStruct<FuncParameter>, ISerializable
    {
        public FuncParameterId GetId()
        {
            return new FuncParameterId(Type);
        }

        public FuncParameter Apply(TypeEnv typeEnv)
        {
            var appliedType = Type.Apply(typeEnv);
            return new FuncParameter(appliedType, Name);
        }

        bool ICyclicEqualityComparableStruct<FuncParameter>.CyclicEquals(ref FuncParameter other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(Type, other.Type)) 
                return false;

            if (!Name.Equals(Name)) 
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(Type), Type);
            context.SerializeRef(nameof(Name), Name);
        }
    }
}