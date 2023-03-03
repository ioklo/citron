using System;
using System.Collections.Generic;
using System.Linq;
using Citron.Collections;
using Citron.Infra;
using Pretune;

using System.Diagnostics;

namespace Citron.Symbol
{   
    public class StructMemberVarDeclSymbol : IDeclSymbolNode, ICyclicEqualityComparableClass<StructMemberVarDeclSymbol>
    {   
        StructDeclSymbol outer;
        Accessor accessor;
        bool bStatic;
        IType declType;
        Name name;

        public StructMemberVarDeclSymbol(StructDeclSymbol outer, Accessor accessor, bool bStatic, IType declType, Name name)
        {
            this.outer = outer;
            this.accessor = accessor;
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

        public IType GetDeclType()
        {   
            return declType;
        }

        void IDeclSymbolNode.AcceptDeclSymbolVisitor<TDeclSymbolNodeVisitor>(ref TDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitStructMemberVar(this);
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
            => other is StructMemberVarDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<StructMemberVarDeclSymbol>.CyclicEquals(StructMemberVarDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(StructMemberVarDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!accessor.Equals(other.accessor))
                return false;

            if (!bStatic.Equals(other.bStatic))
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
            context.SerializeBool(nameof(bStatic), bStatic);
            context.SerializeRef(nameof(declType), declType);
            context.SerializeRef(nameof(name), name);
        }
    }
}