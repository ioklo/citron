using System;
using Citron.Collections;
using Citron.Infra;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Citron.Symbol
{   
    public record ClassMemberFuncDeclSymbol 
        : IFuncDeclSymbol
        , ICyclicEqualityComparableClass<ClassMemberFuncDeclSymbol>
    {
        enum InitializeState
        {
            BeforeInitFuncReturnAndParams,
            AfterInitFuncReturnAndParams,
        }

        ClassDeclSymbol outer;
        Accessor accessor;
        FuncReturn funcReturn;
        Name name;
        ImmutableArray<Name> typeParams;
        ImmutableArray<FuncParameter> parameters;
        bool bStatic;

        CommonFuncDeclSymbolComponent commonComponent;
        LambdaDeclSymbolComponent<ClassMemberFuncDeclSymbol> lambdaComponent;

        InitializeState initState;

        public ClassMemberFuncDeclSymbol(
            ClassDeclSymbol outer, 
            Accessor accessor, 
            Name name,
            ImmutableArray<Name> typeParams,
            bool bStatic)
        {
            this.outer = outer;
            this.accessor = accessor;
            this.name = name;
            this.typeParams = typeParams;
            this.bStatic = bStatic;

            this.commonComponent = new CommonFuncDeclSymbolComponent();
            this.lambdaComponent = new LambdaDeclSymbolComponent<ClassMemberFuncDeclSymbol>(this);
            this.initState = InitializeState.BeforeInitFuncReturnAndParams;
        }

        public void InitFuncReturnAndParams(FuncReturn funcReturn, ImmutableArray<FuncParameter> parameters, bool bLastParamVariadic)
        {
            Debug.Assert(initState == InitializeState.BeforeInitFuncReturnAndParams);

            this.funcReturn = funcReturn;
            this.parameters = parameters;
            this.commonComponent.InitLastParameterVariadic(bLastParamVariadic);

            initState = InitializeState.AfterInitFuncReturnAndParams;
        }

        #region commonComponent
        public bool IsLastParameterVariadic() => commonComponent.IsLastParameterVariadic();
        #endregion commonComponent

        public Accessor GetAccessor()
        {
            return accessor;
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
            return funcReturn;
        }

        public int GetTypeParamCount()
        {   
            return typeParams.Length;
        }

        public Name GetTypeParam(int i)
        {
            return typeParams[i];
        }

        public bool IsStatic()
        {
            return bStatic;
        }

        public DeclSymbolNodeName GetNodeName()
        {   
            return new DeclSymbolNodeName(name, typeParams.Length, parameters.MakeFuncParamIds());
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }

        TResult IDeclSymbolNode.Accept<TDeclSymbolNodeVisitor, TResult>(ref TDeclSymbolNodeVisitor visitor)
            => visitor.VisitClassMemberFunc(this);

        #region IFuncDeclSymbol
        void IFuncDeclSymbol.AddLambda(LambdaDeclSymbol declSymbol) => lambdaComponent.AddLambda(declSymbol);
        bool IFuncDeclSymbol.IsLastParameterVariadic() => IsLastParameterVariadic();
        FuncReturn? IFuncDeclSymbol.GetReturn() => GetReturn();
        #endregion

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is ClassMemberFuncDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<IFuncDeclSymbol>.CyclicEquals(IFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
            => other is ClassMemberFuncDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<ClassMemberFuncDeclSymbol>.CyclicEquals(ClassMemberFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(ClassMemberFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!accessor.Equals(other.accessor))
                return false;

            if (!funcReturn.CyclicEquals(ref other.funcReturn, ref context))
                return false;

            if (!name.Equals(other.name))
                return false;

            if (!typeParams.Equals(other.typeParams))
                return false;

            if (!parameters.CyclicEqualsStructItem(ref other.parameters, ref context))
                return false;

            if (!bStatic.Equals(other.bStatic))
                return false;

            if (!context.CompareStructRef(ref commonComponent, ref other.commonComponent))
                return false;

            if (!context.CompareStructRef(ref lambdaComponent, ref other.lambdaComponent))
                return false;

            if (!initState.Equals(other.initState))
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(outer), outer);
            context.SerializeString(nameof(accessor), accessor.ToString());
            context.SerializeValueRef(nameof(funcReturn), ref funcReturn);
            context.SerializeRef(nameof(name), name);
            context.SerializeRefArray(nameof(typeParams), typeParams);
            context.SerializeValueArray(nameof(parameters), parameters);
            context.SerializeBool(nameof(bStatic), bStatic);
            context.SerializeValueRef(nameof(commonComponent), ref commonComponent);
            context.SerializeValueRef(nameof(lambdaComponent), ref lambdaComponent);
            context.SerializeString(nameof(initState), initState.ToString());
        }
    }
}
