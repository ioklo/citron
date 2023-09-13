using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol;

[AutoConstructor]
public partial class StructType : IType, ICyclicEqualityComparableClass<StructType>
{
    StructSymbol symbol;

    StructType Apply(TypeEnv typeEnv)
    {
        return new StructType(symbol.Apply(typeEnv));
    }

    bool CyclicEquals(StructType other, ref CyclicEqualityCompareContext context)
    {
        if (!context.CompareClass(symbol, other.symbol))
            return false;

        return true;
    }

    IType IType.GetTypeArg(int index) => symbol.GetTypeArg(index);
    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    TypeId IType.GetTypeId() => new SymbolTypeId(symbol.GetSymbolId());
    IType? IType.GetMemberType(Name name, ImmutableArray<IType> typeArgs) => symbol.GetMemberType(name, typeArgs);
    SymbolQueryResult? IType.QueryMember(Name name, int explicitTypeArgCount) => symbol.QueryMember(name, explicitTypeArgCount);

    TResult IType.Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStruct(this);

    bool ICyclicEqualityComparableClass<StructType>.CyclicEquals(StructType other, ref CyclicEqualityCompareContext context)
        => CyclicEquals(other, ref context);

    bool ICyclicEqualityComparableClass<IType>.CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => other is StructType otherType && CyclicEquals(otherType, ref context);

    void ISerializable.DoSerialize(ref SerializeContext context)
    {
        context.SerializeRef(nameof(symbol), symbol);
    }
}
