using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Citron.Symbol
{
    public class ClassMemberVarDeclSymbol : IDeclSymbolNode, ICyclicEqualityComparableClass<ClassMemberVarDeclSymbol>
    {
        ClassDeclSymbol outer;
        Accessor accessModifier;
        bool bStatic;
        IType declType;
        Name name;

        // public static int s;
        public ClassMemberVarDeclSymbol(ClassDeclSymbol outer, Accessor accessModifier, bool bStatic, IType declType, Name name)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.bStatic = bStatic;
            this.declType = declType;
            this.name = name;
        }

        int IDeclSymbolNode.GetTypeParamCount()
        {
            return 0;
        }

        Name IDeclSymbolNode.GetTypeParam(int i)
        {
            throw new RuntimeFatalException();
        }

        public IType GetDeclType()
        {
            return declType;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }        

        public Name GetName()
        {
            return name;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        void IDeclSymbolNode.Accept<TDeclSymbolNodeVisitor>(TDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitClassMemberVar(this);
        }

        void IDeclSymbolNode.Accept<TDeclSymbolNodeVisitor>(ref TDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitClassMemberVar(this);
        }

        public bool IsStatic()
        {
            return bStatic;
        }

        public Accessor GetAccessor()
        {
            return accessModifier;
        }

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is ClassMemberVarDeclSymbol declSymbol && CyclicEquals(declSymbol, ref context);

        bool ICyclicEqualityComparableClass<ClassMemberVarDeclSymbol>.CyclicEquals(ClassMemberVarDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(ClassMemberVarDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!accessModifier.Equals(other.accessModifier))
                return false;

            if (!bStatic.Equals(bStatic))
                return false;

            if (!context.CompareClass(declType, other.declType))
                return false;

            if (!name.Equals(other.name))
                return false;

            return true;
        }
    }
}
