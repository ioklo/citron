using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol;

[AutoConstructor]
public partial class LocalPtrType : IType, ICyclicEqualityComparableClass<LocalPtrType>
{
    IType innerType;

    LocalPtrType Apply(TypeEnv typeEnv)
    {
        var appliedInnerType = innerType.Apply(typeEnv);
        return new LocalPtrType(appliedInnerType);
    }

    bool CyclicEquals(LocalPtrType other, ref CyclicEqualityCompareContext context)
    {
        if (!context.CompareClass(innerType, other.innerType))
            return false;

        return true;
    }

    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    IType IType.GetTypeArg(int index) => throw new RuntimeFatalException();
    TypeId IType.GetTypeId() => new LocalPtrTypeId(innerType.GetTypeId());
    IType? IType.GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
    SymbolQueryResult? IType.QueryMember(Name name, int explicitTypeArgCount) => null;
    TResult IType.Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalPtr(this);

    bool ICyclicEqualityComparableClass<IType>.CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => other is LocalPtrType otherType && CyclicEquals(otherType, ref context);

    bool ICyclicEqualityComparableClass<LocalPtrType>.CyclicEquals(LocalPtrType other, ref CyclicEqualityCompareContext context)
        => CyclicEquals(other, ref context);

    void ISerializable.DoSerialize(ref SerializeContext context)
    {
        context.SerializeRef(nameof(innerType), innerType);
    }
}
