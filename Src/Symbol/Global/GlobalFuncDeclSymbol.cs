﻿using Citron.Collections;
using Pretune;
using System;
using Citron.Infra;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Citron.Symbol
{
    public class GlobalFuncDeclSymbol : IFuncDeclSymbol, ICyclicEqualityComparableClass<GlobalFuncDeclSymbol>, ISerializable
    {
        enum InitializeState
        {
            BeforeInitFuncReturnAndParams,
            AfterInitFuncReturnAndParams,
        }

        // module or namespace
        ITopLevelDeclSymbolNode outer;

        Accessor accessor;
        FuncReturn @return;
        Name name;

        ImmutableArray<Name> typeParams;
        ImmutableArray<FuncParameter> parameters;

        LambdaDeclSymbolComponent<GlobalFuncDeclSymbol> lambdaComponent;

        InitializeState initState;

        public GlobalFuncDeclSymbol(
            ITopLevelDeclSymbolNode outer, Accessor accessModifier, 
            Name name,
            ImmutableArray<Name> typeParams)
        {
            this.outer = outer;
            this.accessor = accessModifier;
            this.name = name;
            this.typeParams = typeParams;

            this.lambdaComponent = new LambdaDeclSymbolComponent<GlobalFuncDeclSymbol>(this);
            this.initState = InitializeState.BeforeInitFuncReturnAndParams;
        }

        public void InitFuncReturnAndParams(FuncReturn @return, ImmutableArray<FuncParameter> parameters)
        {
            Debug.Assert(initState == InitializeState.BeforeInitFuncReturnAndParams);

            this.@return = @return;
            this.parameters = parameters;

            this.initState = InitializeState.AfterInitFuncReturnAndParams;
        }

        public IEnumerable<LambdaDeclSymbol> GetLambdas()
            => lambdaComponent.GetLambdas();

        void IFuncDeclSymbol.AddLambda(LambdaDeclSymbol lambdaDecl)
            => lambdaComponent.AddLambda(lambdaDecl);

        public int GetTypeParamCount()
        {
            return typeParams.Length;
        }

        public Name GetTypeParam(int i)
        {
            return typeParams[i];
        }

        int IDeclSymbolNode.GetTypeParamCount()
            => GetTypeParamCount();

        Name IDeclSymbolNode.GetTypeParam(int i)
            => GetTypeParam(i);
        
        public Accessor GetAccessor()
        {
            return accessor;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            Debug.Assert(InitializeState.BeforeInitFuncReturnAndParams < initState);
            return new DeclSymbolNodeName(name, typeParams.Length, parameters.MakeFuncParamIds());
        }
        
        public int GetParameterCount()
        {
            Debug.Assert(InitializeState.BeforeInitFuncReturnAndParams < initState);
            return parameters.Length;
        }

        public FuncParameter GetParameter(int index)
        {
            Debug.Assert(InitializeState.BeforeInitFuncReturnAndParams < initState);
            return parameters[index];
        }

        public FuncReturn GetReturn()
        {
            Debug.Assert(InitializeState.BeforeInitFuncReturnAndParams < initState);
            return @return;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return typeParams.AsEnumerable().OfType<IDeclSymbolNode>();
        }

        void IDeclSymbolNode.AcceptDeclSymbolVisitor<TDeclSymbolNodeVisitor>(ref TDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitGlobalFunc(this);
        }

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is GlobalFuncDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<IFuncDeclSymbol>.CyclicEquals(IFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
            => other is GlobalFuncDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<GlobalFuncDeclSymbol>.CyclicEquals(GlobalFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(GlobalFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!accessor.Equals(other.accessor))
                return false;

            if (!@return.CyclicEquals(ref other.@return, ref context))
                return false;

            if (!name.Equals(other.name))
                return false;

            if (!typeParams.Equals(other.typeParams))
                return false;

            if (!parameters.CyclicEqualsStructItem(ref other.parameters, ref context))
                return false;

            if (!lambdaComponent.CyclicEquals(ref lambdaComponent, ref context))
                return false;

            if (!initState.Equals(other.initState))
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(outer), outer);
            context.SerializeString(nameof(accessor), accessor.ToString());
            context.SerializeValueRef(nameof(@return), ref @return);
            context.SerializeRef(nameof(name), name);
            context.SerializeRefArray(nameof(typeParams), typeParams);
            context.SerializeValueArray(nameof(parameters), parameters);
            context.SerializeValueRef(nameof(lambdaComponent), ref lambdaComponent);
            context.SerializeString(nameof(initState), initState.ToString());
        }
    }
}