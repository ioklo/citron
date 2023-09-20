using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;
using Pretune;
using System;

namespace Citron.Symbol;

[AutoConstructor]
public partial class EnumType : IType, ICyclicEqualityComparableClass<EnumType>
{
    EnumSymbol symbol;

    public EnumSymbol GetSymbol()
    {
        return symbol;
    }

    public EnumType Apply(TypeEnv typeEnv)
    {
        return new EnumType(symbol.Apply(typeEnv));
    }

    public bool CyclicEquals(EnumType other, ref CyclicEqualityCompareContext context)
    {
        if (!context.CompareClass(symbol, other.symbol))
            return false;

        return true;
    }

    TResult IType.Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnum(this);
    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);

    TypeId IType.GetTypeId() => new SymbolTypeId(IsLocal: false, symbol.GetSymbolId());

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
