using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;

namespace Citron.Symbol;

[AutoConstructor]
public partial class EnumElemType : IType, ICyclicEqualityComparableClass<EnumElemType>
{
    EnumElemSymbol symbol;

    public EnumElemType Apply(TypeEnv typeEnv)
    {
        return new EnumElemType(symbol.Apply(typeEnv));
    }

    public bool CyclicEquals(EnumElemType other, ref CyclicEqualityCompareContext context)
    {
        if (!context.CompareClass(symbol, other.symbol))
            return false;

        return true;
    }

    public EnumType GetEnumType()
    {
        var enumSymbol = symbol.GetOuter();
        return new EnumType(enumSymbol);
    }

    public EnumElemMemberVarSymbol? GetMemberVar(Name name)
    {
        return symbol.GetMemberVar(name);
    }

    TResult IType.Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElem(this);
    IType IType.GetTypeArg(int index) => symbol.GetTypeArg(index);
    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    TypeId IType.GetTypeId() => new SymbolTypeId(symbol.GetSymbolId());
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
