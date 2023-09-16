using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;

namespace Citron.Symbol;

[AutoConstructor]
public partial class LambdaType : IType, ICyclicEqualityComparableClass<LambdaType>
{
    LambdaSymbol symbol;

    public LambdaSymbol GetSymbol() { return symbol; }

    public LambdaType Apply(TypeEnv typeEnv)
    {
        return new LambdaType(symbol.Apply(typeEnv));
    }

    public bool CyclicEquals(LambdaType other, ref CyclicEqualityCompareContext context)
    {
        if (!context.CompareClass(symbol, other.symbol))
            return false;

        return true;
    }

    public SymbolId GetSymbolId()
    {
        return symbol.GetSymbolId();
    }

    public ISymbolNode GetOuter()
    {
        return symbol.GetOuter();
    }

    public int GetParameterCount()
    {
        return symbol.GetParameterCount();
    }

    public FuncParameter GetParameter(int index)
    {
        return symbol.GetParameter(index);
    }

    public FuncReturn GetReturn()
    {
        return symbol.GetReturn();
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
