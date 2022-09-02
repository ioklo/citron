﻿using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using Citron.Module;

namespace Citron.Symbol
{
    public class StructMemberFuncDeclSymbol : IFuncDeclSymbol
    {
        IHolder<StructDeclSymbol> outerHolder;

        AccessModifier accessModifier;
        bool bStatic;
        IHolder<FuncReturn> returnHolder;
        Name name;
        ImmutableArray<TypeVarDeclSymbol> typeParams;
        IHolder<ImmutableArray<FuncParameter>> parametersHolder;

        LambdaDeclSymbolContainerComponent lambdaDeclContainerComponent;

        public void AddLambda(LambdaDeclSymbol lambdaDecl)
            => lambdaDeclContainerComponent.AddLambda(lambdaDecl);

        public StructMemberFuncDeclSymbol(
            IHolder<StructDeclSymbol> outerHolder, 
            AccessModifier accessModifier, 
            bool bStatic, 
            IHolder<FuncReturn> returnHolder,
            Name name,
            ImmutableArray<TypeVarDeclSymbol> typeParams,
            IHolder<ImmutableArray<FuncParameter>> paramsHolder,
            ImmutableArray<LambdaDeclSymbol> lambdaDecls)
        {
            this.outerHolder = outerHolder;
            this.accessModifier = accessModifier;
            this.bStatic = bStatic;
            this.returnHolder = returnHolder;
            this.name = name;
            this.typeParams = typeParams;
            this.parametersHolder = paramsHolder;
            this.lambdaDeclContainerComponent = new LambdaDeclSymbolContainerComponent(lambdaDecls);
        }

        public int GetParameterCount()
        {
            return parametersHolder.GetValue().Length;
        }

        public FuncParameter GetParameter(int index)
        {
            return parametersHolder.GetValue()[index];
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, parametersHolder.GetValue().MakeFuncParamIds());
        }
        
        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return typeParams.AsEnumerable();
        }
        
        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outerHolder.GetValue();
        }

        public int GetTypeParamCount()
        {
            return typeParams.Length;
        }

        public bool IsStatic()
        {
            return bStatic;
        }        

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitStructMemberFunc(this);
        }

        public FuncReturn GetReturn()
        {
            return returnHolder.GetValue();
        }

        public AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }
    }
}