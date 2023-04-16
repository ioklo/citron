using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public partial class ClassConstructorSymbol : IFuncSymbol, ICyclicEqualityComparableClass<ClassConstructorSymbol>
    {
        [ExcludeComparison]
        SymbolFactory factory;

        ClassSymbol outer;
        ClassConstructorDeclSymbol decl;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();
        IFuncSymbol IFuncSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ISymbolNode? ISymbolNode.GetOuter() => GetOuter();

        internal ClassConstructorSymbol(SymbolFactory factory, ClassSymbol outer, ClassConstructorDeclSymbol decl)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
        }

        public int GetParameterCount()
        {
            return decl.GetParameterCount();
        }

        public FuncParameter GetParameter(int index)
        {
            var typeEnv = outer.GetTypeEnv();
            var param = decl.GetParameter(index);
            return param.Apply(typeEnv);
        }

        public ClassSymbol GetOuter()
        {
            return outer;
        }

        public IFuncDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }

        public ClassConstructorSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeClassConstructor(appliedOuter, decl);
        }        

        public IType GetTypeArg(int index)
        {
            throw new RuntimeFatalException();
        }

        public ITypeSymbol? GetOuterType()
        {
            return outer;
        }

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
            => other is ClassConstructorSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<IFuncSymbol>.CyclicEquals(IFuncSymbol other, ref CyclicEqualityCompareContext context)
            => other is ClassConstructorSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<ClassConstructorSymbol>.CyclicEquals(ClassConstructorSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(ClassConstructorSymbol other, ref CyclicEqualityCompareContext context)
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

        public void Accept<TVisitor>(ref TVisitor visitor)
            where TVisitor : struct, ISymbolNodeVisitor
        {
            visitor.VisitClassConstructor(this);
        }

        SymbolQueryResult? ISymbolNode.QueryMember(Name name, int explicitTypeArgCount)
        {
            return null;
        }
    }
}