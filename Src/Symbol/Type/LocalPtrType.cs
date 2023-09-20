using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;

namespace Citron.Symbol;

[AutoConstructor]
public partial class LocalPtrType : IType, ICyclicEqualityComparableClass<LocalPtrType>
{
    IType innerType;

    public LocalPtrType Apply(TypeEnv typeEnv)
    {
        var appliedInnerType = innerType.Apply(typeEnv);
        return new LocalPtrType(appliedInnerType);
    }

    public bool CyclicEquals(LocalPtrType other, ref CyclicEqualityCompareContext context)
    {
        if (!context.CompareClass(innerType, other.innerType))
            return false;

        return true;
    }

    public IType GetInnerType()
    {
        return innerType;
    }

    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    TypeId IType.GetTypeId() => new LocalPtrTypeId(innerType.GetTypeId());
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
