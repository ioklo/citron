using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Citron.Symbol
{
    public class StructConstructorDeclSymbol : IFuncDeclSymbol, ICyclicEqualityComparableClass<StructConstructorDeclSymbol>
    {
        StructDeclSymbol outer;
        Accessor accessor;
        ImmutableArray<FuncParameter> parameters;
        bool bTrivial;

        LambdaDeclSymbolComponent<StructConstructorDeclSymbol> lambdaComponent;

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
            this.accessor = accessModifier;
            this.parameters = parameters;
            this.bTrivial = bTrivial;

            this.lambdaComponent = new LambdaDeclSymbolComponent<StructConstructorDeclSymbol>(this);
        }

        int IDeclSymbolNode.GetTypeParamCount()
        {
            return 0;
        }

        Name IDeclSymbolNode.GetTypeParam(int i)
        {
            throw new RuntimeFatalException();
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(Names.Constructor, 0, parameters.MakeFuncParamIds());
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
            return accessor;
        }

        public bool IsTrivial()
        {
            return bTrivial;
        }

        void IDeclSymbolNode.AcceptDeclSymbolVisitor<TDeclSymbolNodeVisitor>(ref TDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitStructConstructor(this);
        }

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is StructConstructorDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<IFuncDeclSymbol>.CyclicEquals(IFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
            => other is StructConstructorDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<StructConstructorDeclSymbol>.CyclicEquals(StructConstructorDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(StructConstructorDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!accessor.Equals(other.accessor))
                return false;

            if (!parameters.CyclicEqualsStructItem(ref other.parameters, ref context))
                return false;

            if (!bTrivial.Equals(other.bTrivial))
                return false;

            if (!lambdaComponent.CyclicEquals(ref lambdaComponent, ref context))
                return false;

            return true;
        }

        void IFuncDeclSymbol.AddLambda(LambdaDeclSymbol declSymbol)
        {
            throw new NotImplementedException();
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(outer), outer);
            context.SerializeString(nameof(accessor), accessor.ToString());
            context.SerializeValueArray(nameof(parameters), parameters);
            context.SerializeBool(nameof(bTrivial), bTrivial);
            context.SerializeValue(nameof(lambdaComponent), lambdaComponent);
        }

        bool IFuncDeclSymbol.IsLastParameterVariadic()
        {
            throw new NotImplementedException();
        }
    }
}