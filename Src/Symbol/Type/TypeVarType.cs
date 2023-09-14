using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol;

[AutoConstructor]
public partial class TypeVarType : IType, ICyclicEqualityComparableClass<TypeVarType>
{
    int index;
    Name name;

    public IType Apply(TypeEnv typeEnv)
    {
        return typeEnv.GetValue(index);
    }

    public TypeId GetTypeId()
    {
        return new TypeVarTypeId(index, name);
    }
    
    public bool CyclicEquals(TypeVarType other, ref CyclicEqualityCompareContext context)
    {
        if (!index.Equals(other.index))
            return false;

        if (!name.Equals(other.name))
            return false;

        return true;
    }

    IType IType.GetTypeArg(int index) => throw new RuntimeFatalException();
    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    TypeId IType.GetTypeId() => GetTypeId();
    IType? IType.GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
    SymbolQueryResult? IType.QueryMember(Name name, int explicitTypeArgCount) => null;
    TResult IType.Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitTypeVar(this);

    bool ICyclicEqualityComparableClass<IType>.CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => other is TypeVarType otherType && CyclicEquals(otherType, ref context);

    bool ICyclicEqualityComparableClass<TypeVarType>.CyclicEquals(TypeVarType other, ref CyclicEqualityCompareContext context)
        => CyclicEquals(other, ref context);

    void ISerializable.DoSerialize(ref SerializeContext context)
    {
        context.SerializeInt(nameof(index), index);
        context.SerializeRef(nameof(name), name);
    }
}
