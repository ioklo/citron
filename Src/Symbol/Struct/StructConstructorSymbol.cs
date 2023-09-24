using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Diagnostics;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public partial class StructConstructorSymbol : IFuncSymbol, ICyclicEqualityComparableClass<StructConstructorSymbol>
    {
        SymbolFactory factory;
        StructSymbol outer;
        StructConstructorDeclSymbol decl;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ISymbolNode? ISymbolNode.GetOuter() => outer;
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();
        IFuncSymbol IFuncSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        internal StructConstructorSymbol(SymbolFactory factory, StructSymbol outer, StructConstructorDeclSymbol decl)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
        }

        SymbolQueryResult? ISymbolNode.QueryMember(Name name, int explicitTypeArgCount)
        {
            return null;
        }

        public StructConstructorSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeStructConstructor(appliedOuter, decl);
        }

        public IFuncDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public StructSymbol GetOuter()
        {
            return outer;
        }

        public IType GetTypeArg(int index)
        {
            throw new RuntimeFatalException();
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }

        public int GetParameterCount()
        {
            return decl.GetParameterCount();
        }

        public FuncParameter GetParameter(int index)
        {
            var parameter = decl.GetParameter(index);
            var typeEnv = GetTypeEnv();

            return parameter.Apply(typeEnv);
        }

        public ITypeSymbol? GetOuterType()
        {
            return outer;
        }

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
            => other is StructConstructorSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<IFuncSymbol>.CyclicEquals(IFuncSymbol other, ref CyclicEqualityCompareContext context)
            => other is StructConstructorSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<StructConstructorSymbol>.CyclicEquals(StructConstructorSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(StructConstructorSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!context.CompareClass(decl, other.decl))
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(outer), outer);
            context.SerializeRef(nameof(decl), decl);
        }

        TResult IFuncSymbol.Accept<TFuncSymbolVisitor, TResult>(ref TFuncSymbolVisitor visitor)
            => visitor.VisitStructConstructor(this);

        TResult ISymbolNode.Accept<TVisitor, TResult>(ref TVisitor visitor)
            => visitor.VisitStructConstructor(this);
    }
}