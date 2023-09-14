using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;
using Pretune;

namespace Citron.Symbol;

[AutoConstructor]
public partial class EnumType : IType, ICyclicEqualityComparableClass<EnumType>
{
    EnumSymbol symbol;

    EnumType Apply(TypeEnv typeEnv)
    {
        return new EnumType(symbol.Apply(typeEnv));
    }

    bool CyclicEquals(EnumType other, ref CyclicEqualityCompareContext context)
    {
        if (!context.CompareClass(symbol, other.symbol))
            return false;

        return true;
    }

    TResult IType.Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnum(this);
    IType IType.GetTypeArg(int index) => symbol.GetTypeArg(index);
    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);

    TypeId IType.GetTypeId() => new SymbolTypeId(symbol.GetSymbolId());
    IType? IType.GetMemberType(Name name, ImmutableArray<IType> typeArgs)
        => ((ITypeSymbol)symbol).GetMemberType(name, typeArgs);

    SymbolQueryResult? IType.QueryMember(Name name, int explicitTypeArgCount)
        => ((ITypeSymbol)symbol).QueryMember(name, explicitTypeArgCount);

    bool ICyclicEqualityComparableClass<IType>.CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => other is EnumType otherType && CyclicEquals(otherType, ref context);

    void ISerializable.DoSerialize(ref SerializeContext context)
    {
        context.SerializeRef(nameof(symbol), symbol);
    }

    bool ICyclicEqualityComparableClass<EnumType>.CyclicEquals(EnumType other, ref CyclicEqualityCompareContext context)
        => CyclicEquals(other, ref context);
}
