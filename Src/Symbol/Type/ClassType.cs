using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol;

[AutoConstructor]
public partial class ClassType : IType, ICyclicEqualityComparableClass<ClassType>
{
    ClassSymbol symbol;

    ClassType Apply(TypeEnv typeEnv)
    {
        return new ClassType(symbol.Apply(typeEnv));
    }

    bool CyclicEquals(ClassType other, ref CyclicEqualityCompareContext context)
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
    
    TResult IType.Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClass(this);

    bool ICyclicEqualityComparableClass<ClassType>.CyclicEquals(ClassType other, ref CyclicEqualityCompareContext context)
        => CyclicEquals(other, ref context);

    bool ICyclicEqualityComparableClass<IType>.CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        => other is ClassType otherType && CyclicEquals(otherType, ref context);

    void ISerializable.DoSerialize(ref SerializeContext context)
    {
        context.SerializeRef(nameof(symbol), symbol);
    }
}
