using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Citron.Symbol
{
    public class StructMemberFuncDeclSymbol : IFuncDeclSymbol, ICyclicEqualityComparableClass<StructMemberFuncDeclSymbol>
    {
        enum InitializeState
        {
            BeforeInitFuncReturnAndParams,
            AfterInitFuncReturnAndParams,
        }

        StructDeclSymbol outer;

        Accessor accessor;
        bool bStatic;
        FuncReturn funcReturn;
        Name name;
        ImmutableArray<Name> typeParams;
        ImmutableArray<FuncParameter> parameters;

        CommonFuncDeclSymbolComponent commonComponent;
        LambdaDeclSymbolComponent<StructMemberFuncDeclSymbol> lambdaComponent;
        InitializeState initState;

        public StructMemberFuncDeclSymbol(
            StructDeclSymbol outer, 
            Accessor accessModifier,             
            bool bStatic,            
            Name name,
            ImmutableArray<Name> typeParams)
        {
            this.outer = outer;
            this.accessor = accessModifier;            
            this.bStatic = bStatic;            
            this.name = name;
            this.typeParams = typeParams;

            this.commonComponent = new CommonFuncDeclSymbolComponent();
            this.lambdaComponent = new LambdaDeclSymbolComponent<StructMemberFuncDeclSymbol>(this);
            this.initState = InitializeState.BeforeInitFuncReturnAndParams;
        }

        public void InitFuncReturnAndParams(FuncReturn @return, ImmutableArray<FuncParameter> parameters, bool bLastParamVariadic)
        {
            Debug.Assert(initState == InitializeState.BeforeInitFuncReturnAndParams);

            this.funcReturn = @return;
            this.parameters = parameters;
            this.commonComponent.InitLastParameterVariadic(bLastParamVariadic);

            initState = InitializeState.AfterInitFuncReturnAndParams;
        }

        void IFuncDeclSymbol.AddLambda(LambdaDeclSymbol declSymbol)
            => lambdaComponent.AddLambda(declSymbol);

        int IDeclSymbolNode.GetTypeParamCount()
        {
            return typeParams.Length;
        }

        Name IDeclSymbolNode.GetTypeParam(int i)
        {
            return typeParams[i];
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

        public DeclSymbolNodeName GetNodeName()
        {
            Debug.Assert(InitializeState.BeforeInitFuncReturnAndParams < initState);
            return new DeclSymbolNodeName(name, typeParams.Length, parameters.MakeFuncParamIds());
        }
        
        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }
        
        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public int GetTypeParamCount()
        {
            return typeParams.Length;
        }

        public bool IsStatic()
        {
            return bStatic;
        }

        void IDeclSymbolNode.AcceptDeclSymbolVisitor<TDeclSymbolNodeVisitor>(ref TDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitStructMemberFunc(this);
        }

        public FuncReturn GetReturn()
        {
            Debug.Assert(InitializeState.BeforeInitFuncReturnAndParams < initState);
            return funcReturn;
        }

        public Accessor GetAccessor()
        {
            return accessor;
        }

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is StructMemberFuncDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<IFuncDeclSymbol>.CyclicEquals(IFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
            => other is StructMemberFuncDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<StructMemberFuncDeclSymbol>.CyclicEquals(StructMemberFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(StructMemberFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!accessor.Equals(other.accessor))
                return false;

            if (!bStatic.Equals(other.bStatic))
                return false;

            if (!funcReturn.CyclicEquals(ref other.funcReturn, ref context))
                return false;

            if (!name.Equals(other.name))
                return false;

            if (!typeParams.Equals(other.typeParams))
                return false;

            if (!parameters.CyclicEqualsStructItem(ref other.parameters, ref context))
                return false;

            if (!context.CompareStructRef(ref commonComponent, ref other.commonComponent))
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
            context.SerializeBool(nameof(bStatic), bStatic);
            context.SerializeValueRef(nameof(funcReturn), ref funcReturn);
            context.SerializeRef(nameof(name), name);
            context.SerializeRefArray(nameof(typeParams), typeParams);
            context.SerializeValueArray(nameof(parameters), parameters);
            context.SerializeValueRef(nameof(commonComponent), ref commonComponent);
            context.SerializeValueRef(nameof(lambdaComponent), ref lambdaComponent);
            context.SerializeString(nameof(initState), initState.ToString());
        }

        bool IFuncDeclSymbol.IsLastParameterVariadic()
        {
            throw new NotImplementedException();
        }
    }
}