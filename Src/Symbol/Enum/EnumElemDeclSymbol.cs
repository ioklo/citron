using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using Citron.Module;
using System.Diagnostics;
using System.Linq;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public partial class EnumElemDeclSymbol : ITypeDeclSymbol
    {
        enum InitializeState
        {
            BeforeInitMemberVars,
            AfterInitMemberVars
        }

        EnumDeclSymbol outer;
        Name name;
        ImmutableArray<EnumElemMemberVarDeclSymbol> memberVarDecls;

        InitializeState initState;

        public EnumElemDeclSymbol(EnumDeclSymbol outer, Name name)
        {
            this.outer = outer;
            this.name = name;

            this.initState = InitializeState.BeforeInitMemberVars;
        }

        public void InitMemberVars(ImmutableArray<EnumElemMemberVarDeclSymbol> memberVarDecls)
        {
            Debug.Assert(initState == InitializeState.BeforeInitMemberVars);
            Debug.Assert(memberVarDecls.AsEnumerable().All(memberVarDecl => memberVarDecl.GetOuterDeclNode() == outer));

            this.memberVarDecls = memberVarDecls;

            this.initState = InitializeState.AfterInitMemberVars;
        }

        public int GetMemberVarCount()
        {
            Debug.Assert(InitializeState.BeforeInitMemberVars < initState);
            return memberVarDecls.Length;
        }

        public EnumElemMemberVarDeclSymbol GetMemberVar(int index)
        {
            Debug.Assert(InitializeState.BeforeInitMemberVars < initState);
            return memberVarDecls[index];
        }
        
        public bool IsStandalone()
        {
            return memberVarDecls.Length == 0;
        }

        public Name GetName()
        {
            return name;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            Debug.Assert(InitializeState.BeforeInitMemberVars < initState);
            return memberVarDecls.AsEnumerable();
        }

        public void Apply(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitEnumElem(this);
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitEnumElem(this);
        }

        public Accessor GetAccessor()
        {
            return Accessor.Public;
        }
    }
}