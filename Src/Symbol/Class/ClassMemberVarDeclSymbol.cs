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
        Accessor accessor;
        bool bStatic;
        IType declType;
        Name name;

        // public static int s;
        public ClassMemberVarDeclSymbol(ClassDeclSymbol outer, Accessor accessModifier, bool bStatic, IType declType, Name name)
        {
            this.outer = outer;
            this.accessor = accessModifier;
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

        void IDeclSymbolNode.AcceptDeclSymbolVisitor<TDeclSymbolNodeVisitor>(ref TDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitClassMemberVar(this);
        }

        public bool IsStatic()
        {
            return bStatic;
        }

        public Accessor GetAccessor()
        {
            return accessor;
        }

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is ClassMemberVarDeclSymbol declSymbol && CyclicEquals(declSymbol, ref context);

        bool ICyclicEqualityComparableClass<ClassMemberVarDeclSymbol>.CyclicEquals(ClassMemberVarDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(ClassMemberVarDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!accessor.Equals(other.accessor))
                return false;

            if (!bStatic.Equals(bStatic))
                return false;

            if (!context.CompareClass(declType, other.declType))
                return false;

            if (!name.Equals(other.name))
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(outer), outer);
            context.SerializeString(nameof(accessor), accessor.ToString());
            context.SerializeBool(nameof(outer), bStatic);
            context.SerializeRef(nameof(declType), declType);
            context.SerializeRef(nameof(name), name);
        }
    }
}
