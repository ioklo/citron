using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol;

[AutoConstructor]
public partial class VoidType: IType, ICyclicEqualityComparableClass<VoidType>
{
    public override VoidType Apply(TypeEnv typeEnv) => new VoidType();
    public override TypeId GetTypeId() => new VoidTypeId();
    public override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitVoid(this);

    public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => true;

    public override void DoSerialize(ref SerializeContext context)
    {
    }

    public override SymbolQueryResult? QueryMember(Name name, int typeArgCount)
    {
        return null;
    }
}
