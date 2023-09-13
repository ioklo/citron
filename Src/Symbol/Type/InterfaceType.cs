using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol;

// symbol 인터페이스 타입
[AutoConstructor]
public partial class InterfaceType : IInterfaceType, ICyclicEqualityComparableClass<InterfaceType>
{
    InterfaceSymbol symbol;

    public InterfaceType Apply(TypeEnv typeEnv)
    {
        return new InterfaceType(symbol.Apply(typeEnv));
    }

    bool CyclicEquals(InterfaceType other, ref CyclicEqualityCompareContext context)
    {
        if (!context.CompareClass(symbol, other.symbol))
            return false;

        return true;
    }

    bool ICyclicEqualityComparableClass<InterfaceType>.CyclicEquals(InterfaceType other, ref CyclicEqualityCompareContext context)
        => CyclicEquals(other, ref context);

    bool ICyclicEqualityComparableClass<IInterfaceType>.CyclicEquals(IInterfaceType other, ref CyclicEqualityCompareContext context)
        => other is InterfaceType otherType && CyclicEquals(otherType, ref context);

    bool ICyclicEqualityComparableClass<IType>.CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => other is InterfaceType otherType && CyclicEquals(otherType, ref context);

    IType IType.GetTypeArg(int index) => symbol.GetTypeArg(index);
    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);

    TypeId IType.GetTypeId() => new SymbolTypeId(symbol.GetSymbolId());

    IType? IType.GetMemberType(Name name, ImmutableArray<IType> typeArgs)
        => symbol.GetMemberType(name, typeArgs);

    SymbolQueryResult? IType.QueryMember(Name name, int explicitTypeArgCount)
        => symbol.QueryMember(name, explicitTypeArgCount);

    TResult IType.Accept<TVisitor, TResult>(ref TVisitor visitor)
        => visitor.VisitInterface(this);

    void ISerializable.DoSerialize(ref SerializeContext context)
    {
        context.SerializeRef(nameof(symbol), symbol);
    }
}
