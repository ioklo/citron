using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol;

[AutoConstructor]
public partial class NullableType : IType, ICyclicEqualityComparableClass<NullableType>
{
    IType innerType;

    NullableType Apply(TypeEnv typeEnv)
    {
        var appliedInnerType = innerType.Apply(typeEnv);
        return new NullableType(appliedInnerType);
    }

    bool CyclicEquals(NullableType other, ref CyclicEqualityCompareContext context)
    {
        if (!context.CompareClass(innerType, other.innerType))
            return false;

        return true;
    }

    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    IType IType.GetTypeArg(int index) => throw new RuntimeFatalException();
    TypeId IType.GetTypeId() => new NullableTypeId(innerType.GetTypeId());
    IType? IType.GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
    SymbolQueryResult? IType.QueryMember(Name name, int explicitTypeArgCount) => null;
    TResult IType.Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNullable(this);

    bool ICyclicEqualityComparableClass<IType>.CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => other is NullableType otherType && CyclicEquals(otherType, ref context);

    bool ICyclicEqualityComparableClass<NullableType>.CyclicEquals(NullableType other, ref CyclicEqualityCompareContext context)
        => CyclicEquals(other, ref context);

    void ISerializable.DoSerialize(ref SerializeContext context)
    {
        context.SerializeRef(nameof(innerType), innerType);
    }
}
