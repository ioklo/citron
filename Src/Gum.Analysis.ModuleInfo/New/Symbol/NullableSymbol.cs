﻿using Gum.Collections;
using Gum.Infra;
using System;

using M = Gum.CompileTime;

namespace Gum.Analysis
{
    //public class NullableDeclSymbol : ITypeDeclSymbol
    //{
    //    public void Apply(ITypeDeclSymbolVisitor visitor)
    //    {
    //        visitor.VisitNullable(this);
    //    }

    //    public void Apply(IDeclSymbolNodeVisitor visitor)
    //    {
    //        visitor.VisitNullable(this);
    //    }

    //    public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
    //    {
    //        return null;
    //    }

    //    public DeclSymbolNodeName GetNodeName()
    //    {
    //        throw new NotImplementedException();            
    //    }

    //    public IDeclSymbolNode? GetOuterDeclNode()
    //    {
    //        return null;
    //    }
    //}

    // int?
    //public class NullableSymbol : ITypeSymbol
    //{
    //    SymbolFactory factory;
    //    ITypeSymbol innerType;

    //    IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();
    //    ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    //    ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);

    //    public NullableSymbol(SymbolFactory factory, ITypeSymbol innerType)
    //    {
    //        this.factory = factory;
    //        this.innerType = innerType;
    //    }

    //    public ITypeSymbol GetInnerType()
    //    {
    //        return innerType;
    //    }

    //    // T? => int?
    //    public NullableSymbol Apply(TypeEnv typeEnv)
    //    {
    //        var appliedInnerType = innerType.Apply(typeEnv);
    //        return factory.MakeNullable(appliedInnerType);
    //    }
        
    //    public ITypeDeclSymbol GetDeclSymbolNode()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Apply(ITypeSymbolVisitor visitor)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public SymbolQueryResult QueryMember(M.Name memberName, int typeParamCount)
    //    {
    //        throw new UnreachableCodeException();
    //    }

    //    public ISymbolNode? GetOuter()
    //    {
    //        return null;
    //    }        

    //    public TypeEnv GetTypeEnv()
    //    {
    //        return default;
    //    }

    //    public ImmutableArray<ITypeSymbol> GetTypeArgs()
    //    {
    //        return default;
    //    }
    //}
}
