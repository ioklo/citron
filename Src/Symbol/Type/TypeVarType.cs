using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol;

[AutoConstructor]
public partial class TypeVarType : IType, ICyclicEqualityComparableClass<TypeVarType>
{
    int index;
    Name name;

    public override IType Apply(TypeEnv typeEnv) => typeEnv.GetValue(Index);
    public override TypeId GetTypeId() => new TypeVarTypeId(Index, Name);
    public override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitTypeVar(this);

    public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => other is TypeVarType otherType && CyclicEquals(otherType, ref context);

    bool CyclicEquals(TypeVarType other, ref CyclicEqualityCompareContext context)
    {
        if (!Index.Equals(other.Index))
            return false;

        if (!Name.Equals(other.Name))
            return false;

        return true;
    }

    public override void DoSerialize(ref SerializeContext context)
    {
        context.SerializeInt(nameof(Index), Index);
        context.SerializeRef(nameof(Name), Name);
    }

    public override SymbolQueryResult? QueryMember(Name name, int typeArgCount)
    {
        return null;
    }
}
