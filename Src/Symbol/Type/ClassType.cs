using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;

namespace Citron.Symbol;

[AutoConstructor]
public partial class ClassType : IType, ICyclicEqualityComparableClass<ClassType>
{
    ClassSymbol symbol;

    public ClassType Apply(TypeEnv typeEnv)
    {
        return new ClassType(symbol.Apply(typeEnv));
    }
    
    public ClassSymbol GetSymbol()
    {
        return symbol;
    }

    public ClassDeclSymbol GetDeclSymbol()
    {
        return symbol.GetDecl();
    }

    public SymbolQueryResult? QueryMember(Name memberName, int explicitTypeArgsCount)
    {
        return symbol.QueryMember(memberName, explicitTypeArgsCount);
    }

    public ClassMemberVarSymbol? GetMemberVar(Name name)
    {
        return symbol.GetMemberVar(name);
    }

    public ClassConstructorSymbol? GetTrivialConstructor()
    {
        return symbol.GetTrivialConstructor();
    }

    public bool CyclicEquals(ClassType other, ref CyclicEqualityCompareContext context)
    {
        if (!context.CompareClass(symbol, other.symbol))
            return false;

        return true;
    }
    
    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    TypeId IType.GetTypeId() => new SymbolTypeId(IsLocal: false, symbol.GetSymbolId());
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
