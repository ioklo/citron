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

    void ISerializable.DoSerialize(ref SerializeContext context)
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

    public int GetMemberVarCount()
    {
        return memberVars.Length;
    }

    public TupleMemberVar GetMemberVar(int index)
    {
        return memberVars[index];
    }

    public TupleType Apply(TypeEnv typeEnv)
    {
        var builder = ImmutableArray.CreateBuilder<TupleMemberVar>(memberVars.Length);
        foreach (var memberVar in memberVars)
            builder.Add(memberVar.Apply(typeEnv));
        return new TupleType(builder.MoveToImmutable());
    }

    public TupleTypeId GetTypeId()
    {
        var builder = ImmutableArray.CreateBuilder<TupleMemberVarId>(memberVars.Length);
        foreach (var memberVar in memberVars)
            builder.Add(new TupleMemberVarId(memberVar.GetDeclType().GetTypeId(), memberVar.GetName()));
        return new TupleTypeId(builder.MoveToImmutable());
    }   

    public bool CyclicEquals(TupleType other, ref CyclicEqualityCompareContext context)
    {
        return memberVars.CyclicEqualsStructItem(ref other.memberVars, ref context);
    }
    
    public SymbolQueryResult? QueryMember(Name name, int explicitTypeArgCount)
    {
        if (explicitTypeArgCount != 0) return null;

        foreach (var memberVar in memberVars)
            if (memberVar.GetName().Equals(name))
                return new SymbolQueryResult.TupleMemberVar();

        return null;
    }

    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    TypeId IType.GetTypeId() => GetTypeId();
    SymbolQueryResult? IType.QueryMember(Name name, int explicitTypeArgCount) => QueryMember(name, explicitTypeArgCount);
    TResult IType.Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitTuple(this);

    bool ICyclicEqualityComparableClass<TupleType>.CyclicEquals(TupleType other, ref CyclicEqualityCompareContext context)
        => CyclicEquals(other, ref context);

    bool ICyclicEqualityComparableClass<IType>.CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => other is TupleType otherType && CyclicEquals(otherType, ref context);

    void ISerializable.DoSerialize(ref SerializeContext context)
    {
        context.SerializeValueArray(nameof(memberVars), memberVars);
    }
}
