using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol;

public struct TupleMemberVar : ISerializable, ICyclicEqualityComparableStruct<TupleMemberVar>
{
    IType declType;
    Name name;

    public TupleMemberVar(IType declType, Name name)
    {
        this.declType = declType;
        this.name = name;
    }

    public IType GetDeclType()
    {
        return declType;
    }

    public Name GetName()
    {
        return name;
    }

    public TupleMemberVar Apply(TypeEnv typeEnv)
    {
        return new TupleMemberVar(declType.Apply(typeEnv), name);
    }

    public void DoSerialize(ref SerializeContext context)
    {
        context.SerializeRef(nameof(declType), declType);
        context.SerializeRef(nameof(name), name);
    }

    bool ICyclicEqualityComparableStruct<TupleMemberVar>.CyclicEquals(ref TupleMemberVar other, ref CyclicEqualityCompareContext context)
    {
        if (!context.CompareClass(this.GetDeclType(), other.declType))
            return false;

        if (!name.Equals(other.name))
            return false;

        return true;
    }
}

[AutoConstructor]
public partial class TupleType : IType, ICyclicEqualityComparableClass<TupleType>
{
    ImmutableArray<TupleMemberVar> memberVars;

    public TupleType(ImmutableArray<TupleMemberVar> memberVars)
    {
        this.memberVars = memberVars;
    }

    public int GetMemberVarCount()
    {
        return memberVars.Length;
    }

    public TupleMemberVar GetMemberVar(int index)
    {
        return memberVars[index];
    }

    public override IType Apply(TypeEnv typeEnv)
    {
        var builder = ImmutableArray.CreateBuilder<TupleMemberVar>(memberVars.Length);
        foreach (var memberVar in memberVars)
            builder.Add(memberVar.Apply(typeEnv));
        return new TupleType(builder.MoveToImmutable());
    }

    public override TypeId GetTypeId()
    {
        var builder = ImmutableArray.CreateBuilder<TupleMemberVarId>(memberVars.Length);
        foreach (var memberVar in memberVars)
            builder.Add(new TupleMemberVarId(memberVar.GetDeclType().GetTypeId(), memberVar.GetName()));
        return new TupleTypeId(builder.MoveToImmutable());
    }
    
    public override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitTuple(this);

    public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => other is TupleType otherType && CyclicEquals(otherType, ref context);

    bool CyclicEquals(TupleType other, ref CyclicEqualityCompareContext context)
    {
        return memberVars.CyclicEqualsStructItem(ref other.memberVars, ref context);
    }

    public override void DoSerialize(ref SerializeContext context)
    {
        context.SerializeValueArray(nameof(memberVars), memberVars);
    }

    public override SymbolQueryResult? QueryMember(Name name, int typeArgCount)
    {
        foreach (var memberVar in memberVars)
            if (memberVar.GetName().Equals(name))
                return new SymbolQueryResult.TupleMemberVar();

        return null;
    }
}
