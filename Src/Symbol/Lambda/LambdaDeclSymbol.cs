using System;
using System.Collections.Generic;
using System.Diagnostics;
using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol
{   
    public class LambdaDeclSymbol : ITypeDeclSymbol, IFuncDeclSymbol, ICyclicEqualityComparableClass<LambdaDeclSymbol>
    {
        enum InitializeState
        {
            BeforeInitReturn,
            AfterInitFuncReturn
        }

        IFuncDeclSymbol outer;
        Name name;

        // Invoke 함수 시그니처
        FuncReturn @return;
        ImmutableArray<FuncParameter> parameters;

        // 가지고 있어야 할 멤버 변수들, type, name, ref 여부
        List<LambdaMemberVarDeclSymbol> memberVars;

        // Lambda가 Lambda를 갖고 있을 때,
        LambdaDeclSymbolComponent<LambdaDeclSymbol> lambdaComponent;

        InitializeState initState;

        public LambdaDeclSymbol(
            IFuncDeclSymbol outer,
            Name name,
            ImmutableArray<FuncParameter> parameters)
        {
            this.outer = outer;
            this.name = name;
            this.parameters = parameters;

            this.memberVars = new List<LambdaMemberVarDeclSymbol>();
            this.lambdaComponent = new LambdaDeclSymbolComponent<LambdaDeclSymbol>(this);

            this.initState = InitializeState.BeforeInitReturn;
        }

        public void InitReturn(FuncReturn @return)
        {
            Debug.Assert(initState == InitializeState.BeforeInitReturn);
            this.@return = @return;

            initState = InitializeState.AfterInitFuncReturn;
        }

        public IEnumerable<LambdaDeclSymbol> GetLambdas()
            => lambdaComponent.GetLambdas();

        public void AddLambda(LambdaDeclSymbol lambdaDecl)
            => lambdaComponent.AddLambda(lambdaDecl);

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return memberVars;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public int GetMemberVarCount()
        {
            return memberVars.Count;
        }

        public LambdaMemberVarDeclSymbol GetMemberVar(int index)
        {
            return memberVars[index];
        }

        public Accessor GetAccessor()
        {
            return Accessor.Public;
        }

        public FuncReturn GetReturn()
        {
            Debug.Assert(InitializeState.BeforeInitReturn < initState);
            return @return;
        }

        public int GetParameterCount()
        {
            return parameters.Length;
        }

        public FuncParameter GetParameter(int index)
        {
            return parameters[index];
        }

        void IDeclSymbolNode.AcceptDeclSymbolVisitor<TDeclSymbolNodeVisitor>(ref TDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitLambda(this);
        }

        void ITypeDeclSymbol.AcceptTypeDeclSymbolVisitor<TTypeDeclSymbolVisitor>(ref TTypeDeclSymbolVisitor visitor)
        {
            visitor.VisitLambda(this);
        }

        int IDeclSymbolNode.GetTypeParamCount()
        {
            return 0;
        }

        Name IDeclSymbolNode.GetTypeParam(int i)
        {
            throw new RuntimeFatalException();
        }

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is LambdaDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<ITypeDeclSymbol>.CyclicEquals(ITypeDeclSymbol other, ref CyclicEqualityCompareContext context)
            => other is LambdaDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<IFuncDeclSymbol>.CyclicEquals(IFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
            => other is LambdaDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<LambdaDeclSymbol>.CyclicEquals(LambdaDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(LambdaDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!name.Equals(other.name))
                return false;

            if (!@return.CyclicEquals(ref other.@return, ref context))
                return false;

            if (!parameters.CyclicEqualsStructItem(ref other.parameters, ref context))
                return false;

            if (!memberVars.CyclicEqualsClassItem(other.memberVars, ref context))
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
            context.SerializeRef(nameof(name), name);
            context.SerializeValueRef(nameof(@return), ref @return);
            context.SerializeValueArray(nameof(parameters), parameters);
            context.SerializeRefList(nameof(memberVars), memberVars);
            context.SerializeValueRef(nameof(lambdaComponent), ref lambdaComponent);
            context.SerializeString(nameof(initState), initState.ToString());
        }
    }
}
