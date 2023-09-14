using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol;

[AutoConstructor]
public partial class BoxPtrType : IType, ICyclicEqualityComparableClass<BoxPtrType>
{
    IType innerType;

    public IType GetInnerType()
    {
        return innerType;
    }

    public BoxPtrType Apply(TypeEnv typeEnv)
    {
        var appliedInnerType = innerType.Apply(typeEnv);
        return new BoxPtrType(appliedInnerType);
    }

    public bool CyclicEquals(BoxPtrType other, ref CyclicEqualityCompareContext context)
    {
        if (!context.CompareClass(innerType, other.innerType))
            return false;

        return true;
    }

    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    IType IType.GetTypeArg(int index) => throw new RuntimeFatalException();
    TypeId IType.GetTypeId() => new BoxPtrTypeId(innerType.GetTypeId());
    IType? IType.GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
    SymbolQueryResult? IType.QueryMember(Name name, int explicitTypeArgCount) => null;
    TResult IType.Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoxPtr(this);

    bool ICyclicEqualityComparableClass<IType>.CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => other is BoxPtrType otherType && CyclicEquals(otherType, ref context);

    bool ICyclicEqualityComparableClass<BoxPtrType>.CyclicEquals(BoxPtrType other, ref CyclicEqualityCompareContext context)
        => CyclicEquals(other, ref context);

    void ISerializable.DoSerialize(ref SerializeContext context)
    {
        context.SerializeRef(nameof(innerType), innerType);
    }
}
