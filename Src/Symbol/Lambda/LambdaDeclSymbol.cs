using System;
using System.Collections.Generic;
using Citron.Collections;
using Citron.Infra;
using Pretune;
using Citron.Module;

namespace Citron.Symbol
{   
    public class LambdaDeclSymbol : ITypeDeclSymbol, IFuncDeclSymbol
    {
        IHolder<IFuncDeclSymbol> outerHolder; // LambdaDecl이 LambdaDecl을 갖고 있을 때, Holder가 필요하다
        Name name;

        // Invoke 함수 시그니처
        FuncReturn @return;
        ImmutableArray<FuncParameter> parameters;

        // 가지고 있어야 할 멤버 변수들, type, name, ref 여부
        ImmutableArray<LambdaMemberVarDeclSymbol> memberVars;

        LambdaDeclSymbolContainerComponent lambdaDeclContainerComponent;

        public void AddLambda(LambdaDeclSymbol lambdaDecl)
            => lambdaDeclContainerComponent.AddLambda(lambdaDecl);

        public LambdaDeclSymbol(
            IHolder<IFuncDeclSymbol> outerHolder,
            Name name,
            FuncReturn @return,
            ImmutableArray<FuncParameter> parameters,            
            ImmutableArray<LambdaMemberVarDeclSymbol> memberVars,
            ImmutableArray<LambdaDeclSymbol> lambdaDecls)
        {
            this.outerHolder = outerHolder;
            this.name = name;
            this.@return = @return;
            this.parameters = parameters;
            this.memberVars = memberVars;

            this.lambdaDeclContainerComponent = new LambdaDeclSymbolContainerComponent(lambdaDecls);
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return memberVars.AsEnumerable();
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outerHolder.GetValue();
        }

        public int GetMemberVarCount()
        {
            return memberVars.Length;
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

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitLambda(this);
        }

        public void Apply(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitLambda(this);
        }
    }
}
