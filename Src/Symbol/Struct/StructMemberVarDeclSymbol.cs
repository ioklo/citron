using System;
using System.Collections.Generic;
using System.Linq;
using Citron.Collections;
using Citron.Infra;
using Pretune;

using Citron.Module;
using System.Diagnostics;

namespace Citron.Symbol
{   
    public partial class StructMemberVarDeclSymbol : IDeclSymbolNode
    {
        enum InitializeState
        {
            BeforeSettingDeclType,
            Done,
        }

        StructDeclSymbol outer;
        Accessor accessor;
        bool bStatic;
        ITypeSymbol? declType;
        Name name;

        InitializeState initState;

        public StructMemberVarDeclSymbol(StructDeclSymbol outer, Accessor accessor, bool bStatic, Name name)
        {
            this.outer = outer;
            this.accessor = accessor;
            this.bStatic = bStatic;
            this.name = name;
            this.initState = InitializeState.BeforeSettingDeclType;
        }

        public void SetDeclType(ITypeSymbol declType)
        {
            Debug.Assert(initState == InitializeState.BeforeSettingDeclType);
            this.declType = declType;
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

        public ITypeSymbol GetDeclType()
        {
            Debug.Assert(initState == InitializeState.Done);
            return declType!;
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
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
    }
}