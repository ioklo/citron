using Citron.Collections;
using System;

using Citron.Infra;
using System.Collections.Generic;
using System.Linq;

namespace Citron.Symbol
{
    public class ClassConstructorDeclSymbol : IFuncDeclSymbol, ICyclicEqualityComparableClass<ClassConstructorDeclSymbol>
    {
        ClassDeclSymbol outer;
        Accessor accessor;
        ImmutableArray<FuncParameter> parameters;
        bool bTrivial;

        CommonFuncDeclSymbolComponent commonComponent;
        LambdaDeclSymbolComponent<ClassConstructorDeclSymbol> lambdaComponent;

        public ClassConstructorDeclSymbol(ClassDeclSymbol outer, Accessor accessor, ImmutableArray<FuncParameter> parameters, bool bTrivial, bool bLastParamVariadic)
        {
            this.outer = outer;
            this.accessor = accessor;
            this.parameters = parameters;
            this.bTrivial = bTrivial;

            this.commonComponent = new CommonFuncDeclSymbolComponent(bLastParamVariadic);
            this.lambdaComponent = new LambdaDeclSymbolComponent<ClassConstructorDeclSymbol>(this);
        }

        int IDeclSymbolNode.GetTypeParamCount()
        {
            return 0;
        }

        Name IDeclSymbolNode.GetTypeParam(int i)
        {
            throw new RuntimeFatalException();
        }

        public Accessor GetAccessor()
        {
            return accessor;
        }

        public int GetParameterCount()
        {
            return parameters.Length;
        }

        public FuncParameter GetParameter(int index)
        {
            return parameters[index];
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
        
        void IDeclSymbolNode.AcceptDeclSymbolVisitor<TDeclSymbolNodeVisitor>(ref TDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitClassConstructor(this);
        }

        public bool IsTrivial()
        {
            return bTrivial;
        }

        bool CyclicEquals(ClassConstructorDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            // 레퍼런스는 이렇게
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!accessor.Equals(other.accessor))
                return false;

            // struct는 이렇게
            if (!parameters.CyclicEqualsStructItem(ref other.parameters, ref context))
                return false;

            if (!bTrivial.Equals(other.bTrivial))
                return false;

            if (!context.CompareStructRef(ref commonComponent, ref other.commonComponent))
                return false;

            if (!context.CompareStructRef(ref lambdaComponent, ref other.lambdaComponent))
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(outer), outer);
            context.SerializeString(nameof(accessor), accessor.ToString());
            context.SerializeValueArray(nameof(parameters), parameters);
            context.SerializeBool(nameof(bTrivial), bTrivial);
            context.SerializeValueRef(nameof(commonComponent), ref commonComponent);
            context.SerializeValueRef(nameof(lambdaComponent), ref lambdaComponent);
        }

        bool ICyclicEqualityComparableClass<ClassConstructorDeclSymbol>.CyclicEquals(ClassConstructorDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is ClassConstructorDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<IFuncDeclSymbol>.CyclicEquals(IFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
            => other is ClassConstructorDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        void IFuncDeclSymbol.AddLambda(LambdaDeclSymbol declSymbol)
            => lambdaComponent.AddLambda(declSymbol);

        bool IFuncDeclSymbol.IsLastParameterVariadic() => commonComponent.IsLastParameterVariadic();
    }
}
