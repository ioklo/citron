using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol;

[AutoConstructor]
public partial class VoidType: IType, ICyclicEqualityComparableClass<VoidType>
{
    public VoidTypeId GetTypeId()
    {
        return new VoidTypeId();
    }

    public bool CyclicEquals(VoidType other, ref CyclicEqualityCompareContext context)
    {
        return true;
    }
    
    IType IType.Apply(TypeEnv typeEnv) => this;
    TypeId IType.GetTypeId() => new VoidTypeId();
    SymbolQueryResult? IType.QueryMember(Name name, int explicitTypeArgCount) => null;
    TResult IType.Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitVoid(this);

    bool ICyclicEqualityComparableClass<IType>.CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => other is VoidType otherType && CyclicEquals(otherType, ref context);

    bool ICyclicEqualityComparableClass<VoidType>.CyclicEquals(VoidType other, ref CyclicEqualityCompareContext context)
        => CyclicEquals(other, ref context);

    void ISerializable.DoSerialize(ref SerializeContext context)
    {
        // do nothing
    }
}
