using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol;

[AutoConstructor]
public partial class LambdaType : IType, ICyclicEqualityComparableClass<LambdaType>
{
    LambdaSymbol symbol;

    LambdaType Apply(TypeEnv typeEnv)
    {
        return new LambdaType(symbol.Apply(typeEnv));
    }

    bool CyclicEquals(LambdaType other, ref CyclicEqualityCompareContext context)
    {
        if (!context.CompareClass(symbol, other.symbol))
            return false;

        return true;
    }

    TResult IType.Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLambda(this);
    IType IType.GetTypeArg(int index) => symbol.GetTypeArg(index);
    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);

    TypeId IType.GetTypeId() => new SymbolTypeId(symbol.GetSymbolId());
    IType? IType.GetMemberType(Name name, ImmutableArray<IType> typeArgs)
        => symbol.GetMemberType(name, typeArgs);

    SymbolQueryResult? IType.QueryMember(Name name, int explicitTypeArgCount)
        => symbol.QueryMember(name, explicitTypeArgCount);

    bool ICyclicEqualityComparableClass<IType>.CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => other is LambdaType otherType && CyclicEquals(otherType, ref context);

    void ISerializable.DoSerialize(ref SerializeContext context)
    {
        context.SerializeRef(nameof(symbol), symbol);
    }

    bool ICyclicEqualityComparableClass<LambdaType>.CyclicEquals(LambdaType other, ref CyclicEqualityCompareContext context)
        => CyclicEquals(other, ref context);
}
