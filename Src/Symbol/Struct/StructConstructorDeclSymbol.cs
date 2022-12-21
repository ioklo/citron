using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using Citron.Module;

namespace Citron.Symbol
{
    public class StructConstructorDeclSymbol : IFuncDeclSymbol
    {
        StructDeclSymbol outer;
        Accessor accessModifier;
        ImmutableArray<FuncParameter> parameters;
        bool bTrivial;

        // 분석중에 추가되는 LambdaDeclSymbol, 분석이 backtracking 될 수 있기 때문에 롤백할수 있어야 한다
        // 새 객체를 만드는 방식으로 바꾸지는 않아야 한다, 참조하는 곳이 많다

        // void G(func<int, int> f, int i) {...}      // 1
        // void G(func<byte, byte> f, string s) {...} // 2
        // void F()
        // {
        //     G(e => e, 2); // 인자에 맞는 함수를 검색하면서 e => e가 두번 평가된다
        // }

        public StructConstructorDeclSymbol(StructDeclSymbol outer, Accessor accessModifier, ImmutableArray<FuncParameter> parameters, bool bTrivial)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.parameters = parameters;
            this.bTrivial = bTrivial;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(Name.Constructor, 0, parameters.MakeFuncParamIds());
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }

        public int GetParameterCount()
        {
            return parameters.Length;
        }

        public FuncParameter GetParameter(int index)
        {
            return parameters[index];
        }

        public Accessor GetAccessor()
        {
            return accessModifier;
        }

        public bool IsTrivial()
        {
            return bTrivial;
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitStructConstructor(this);
        }
    }
}