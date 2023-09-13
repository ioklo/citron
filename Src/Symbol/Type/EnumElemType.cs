using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol;

[AutoConstructor]
public partial class EnumElemType : IType, ICyclicEqualityComparableClass<EnumElemType>
{
    EnumElemSymbol symbol;

    EnumElemType Apply(TypeEnv typeEnv)
    {
        return new EnumElemType(symbol.Apply(typeEnv));
    }

    bool CyclicEquals(EnumElemType other, ref CyclicEqualityCompareContext context)
    {
        if (!context.CompareClass(symbol, other.symbol))
            return false;

        return true;
    }

    TResult IType.Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElem(this);
    IType IType.GetTypeArg(int index) => symbol.GetTypeArg(index);
    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    TypeId IType.GetTypeId() => symbol.GetSymbolId();
    IType? IType.GetMemberType(Name name, ImmutableArray<IType> typeArgs)
        => ((ITypeSymbol)symbol).GetMemberType(name, typeArgs);

    SymbolQueryResult? IType.QueryMember(Name name, int explicitTypeArgCount)
        => ((ITypeSymbol)symbol).QueryMember(name, explicitTypeArgCount);

    bool ICyclicEqualityComparableClass<IType>.CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => other is EnumElemType otherType && CyclicEquals(otherType, ref context);

    bool ICyclicEqualityComparableClass<EnumElemType>.CyclicEquals(EnumElemType other, ref CyclicEqualityCompareContext context)
        => CyclicEquals(other, ref context);

    void ISerializable.DoSerialize(ref SerializeContext context)
    {
        context.SerializeRef(nameof(symbol), symbol);
    }
}
