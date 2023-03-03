using System;
using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public partial class StructMemberVarSymbol : ISymbolNode, ICyclicEqualityComparableClass<StructMemberVarSymbol>
    {
        SymbolFactory factory;
        StructSymbol outer;
        StructMemberVarDeclSymbol decl;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        internal StructMemberVarSymbol(SymbolFactory factory, StructSymbol outer, StructMemberVarDeclSymbol decl)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
        }

        SymbolQueryResult ISymbolNode.QueryMember(Name name, int explicitTypeArgCount)
        {
            return SymbolQueryResults.NotFound;
        }

        public StructMemberVarSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeStructMemberVar(appliedOuter, decl);
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public Name GetName()
        {
            return decl.GetName();
        }

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public IType GetTypeArg(int index)
        {
            throw new RuntimeFatalException();
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }

        public IType GetDeclType()
        {
            var declType = decl.GetDeclType();
            var typeEnv = GetTypeEnv();

            return declType.Apply(typeEnv);
        }

        public bool IsStatic()
        {
            return decl.IsStatic();
        }

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
            => other is StructMemberVarSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<StructMemberVarSymbol>.CyclicEquals(StructMemberVarSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(StructMemberVarSymbol other, ref CyclicEqualityCompareContext context)
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
            visitor.VisitStructMemberVar(this);
        }
    }
}