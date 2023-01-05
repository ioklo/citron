using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using Citron.Module;
using System.Diagnostics;

namespace Citron.Symbol
{
    public class EnumElemMemberVarDeclSymbol : IDeclSymbolNode, ICyclicEqualityComparableClass<EnumElemMemberVarDeclSymbol>
    {
        enum InitializeState
        {
            BeforeInitDeclType,
            AfterInitDeclType
        }

        EnumElemDeclSymbol outer;
        IType? declType;
        Name name;

        InitializeState initState;

        public EnumElemMemberVarDeclSymbol(EnumElemDeclSymbol outer, Name name)
        {
            this.outer = outer;
            this.name = name;

            this.initState = InitializeState.BeforeInitDeclType;
        }

        int IDeclSymbolNode.GetTypeParamCount()
        {
            return 0;
        }

        Name IDeclSymbolNode.GetTypeParam(int i)
        {
            throw new RuntimeFatalException();
        }

        public void InitDeclType(IType declType)
        {
            Debug.Assert(InitializeState.BeforeInitDeclType == initState);
            this.declType = declType;
            initState = InitializeState.AfterInitDeclType;
        }

        public Name GetName()
        {
            return name;
        }

        public Name GetTypeParam(int i)
        {
            throw new RuntimeFatalException();
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public IType GetDeclType()
        {
            Debug.Assert(InitializeState.BeforeInitDeclType < initState);
            return declType!;
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitEnumElemMemberVar(this);
        }

        public Accessor GetAccessor()
        {
            return Accessor.Public;
        }

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is EnumElemMemberVarDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<EnumElemMemberVarDeclSymbol>.CyclicEquals(EnumElemMemberVarDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(EnumElemMemberVarDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!context.CompareClass(declType, other.declType))
                return false;

            if (!name.Equals(other.name))
                return false;

            if (!initState.Equals(other.initState))
                return false;

            return true;
        }
    }
}