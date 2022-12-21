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
    public class EnumElemMemberVarDeclSymbol : IDeclSymbolNode
    {
        enum InitializeState
        {
            BeforeInitDeclType,
            AfterInitDeclType
        }

        EnumElemDeclSymbol outer;
        ITypeSymbol? declType;
        Name name;

        InitializeState initState;

        public EnumElemMemberVarDeclSymbol(EnumElemDeclSymbol outer, Name name)
        {
            this.outer = outer;
            this.name = name;

            this.initState = InitializeState.BeforeInitDeclType;
        }

        public void InitDeclType(ITypeSymbol declType)
        {
            Debug.Assert(InitializeState.BeforeInitDeclType == initState);
            this.declType = declType;
            initState = InitializeState.AfterInitDeclType;
        }

        public Name GetName()
        {
            return name;
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

        public ITypeSymbol GetDeclType()
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
    }
}