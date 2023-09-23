﻿using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;

namespace Citron.Symbol;

[AutoConstructor]
public partial class StructType : IType, ICyclicEqualityComparableClass<StructType>
{
    StructSymbol symbol;

    public StructType Apply(TypeEnv typeEnv)
    {
        return new StructType(symbol.Apply(typeEnv));
    }

    public SymbolTypeId GetTypeId()
    {
        return new SymbolTypeId(IsLocal: false, symbol.GetSymbolId());
    }

    public StructSymbol GetSymbol()
    {
        return symbol;
    }

    public bool CyclicEquals(StructType other, ref CyclicEqualityCompareContext context)
    {
        if (!context.CompareClass(symbol, other.symbol))
            return false;

        return true;
    }

    public StructDeclSymbol GetDeclSymbol()
    {
        return symbol.GetDecl();
    }

    public StructConstructorSymbol? GetTrivialConstructor()
    {
        return symbol.GetTrivialConstructor();
    }

    public StructMemberVarSymbol? GetMemberVar(Name name)
    {
        return symbol.GetMemberVar(name);
    }

    public StructMemberFuncSymbol? GetMemberFunc(Name name, ImmutableArray<IType> typeArgs, ImmutableArray<FuncParamId> paramIds)
    {
        return symbol.GetMemberFunc(name, typeArgs, paramIds);
    }

    IType IType.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    TypeId IType.GetTypeId() => GetTypeId();
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