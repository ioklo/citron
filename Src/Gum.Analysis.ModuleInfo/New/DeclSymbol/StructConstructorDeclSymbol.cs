using Gum.Collections;
using Gum.Infra;
using System;
using System.Collections.Generic;
using M = Gum.CompileTime;

namespace Citron.Analysis
{
    public class StructConstructorDeclSymbol : IFuncDeclSymbol
    {
        IHolder<StructDeclSymbol> outer;
        M.AccessModifier accessModifier;
        IHolder<ImmutableArray<FuncParameter>> parametersHolder;
        bool bTrivial;

        // 분석중에 추가되는 LambdaDeclSymbol, 분석이 backtracking 될 수 있기 때문에 롤백할수 있어야 한다
        // 새 객체를 만드는 방식으로 바꾸지는 않아야 한다, 참조하는 곳이 많다

        // void G(func<int, int> f, int i) {...}      // 1
        // void G(func<byte, byte> f, string s) {...} // 2
        // void F()
        // {
        //     G(e => e, 2); // 인자에 맞는 함수를 검색하면서 e => e가 두번 평가된다
        // }

        List<LambdaDeclSymbol> lambdaDecls;

        public StructConstructorDeclSymbol(IHolder<StructDeclSymbol> outer, M.AccessModifier accessModifier, IHolder<ImmutableArray<FuncParameter>> parametersHolder, bool bTrivial)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.parametersHolder = parametersHolder;
            this.bTrivial = bTrivial;

            this.lambdaDecls = new List<LambdaDeclSymbol>();
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(M.Name.Constructor, 0, parametersHolder.GetValue().MakeFuncParamIds());
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<M.FuncParamId> paramIds)
        {
            return null;
        }

        public int GetParameterCount()
        {
            return parametersHolder.GetValue().Length;
        }

        public FuncParameter GetParameter(int index)
        {
            return parametersHolder.GetValue()[index];
        }

        public M.AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitStructConstructor(this);
        }

        public void AddLambda(LambdaDeclSymbol decl)
        {
            lambdaDecls.Add(decl);
        }
    }
}