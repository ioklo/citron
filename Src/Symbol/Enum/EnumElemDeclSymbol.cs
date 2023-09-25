using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Citron.Symbol
{   
    public class EnumElemDeclSymbol : ITypeDeclSymbol, ICyclicEqualityComparableClass<EnumElemDeclSymbol>
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

        int IDeclSymbolNode.GetTypeParamCount()
        {
            return 0;
        }

        Name IDeclSymbolNode.GetTypeParam(int i)
        {
            throw new RuntimeFatalException();
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

        TResult ITypeDeclSymbol.Accept<TTypeDeclSymbolVisitor, TResult>(ref TTypeDeclSymbolVisitor visitor)
            => visitor.VisitEnumElem(this);
        
        TResult IDeclSymbolNode.Accept<TDeclSymbolNodeVisitor, TResult>(ref TDeclSymbolNodeVisitor visitor)
            => visitor.VisitEnumElem(this);

        public Accessor GetAccessor()
        {
            return Accessor.Public;
        }

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is EnumElemDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<ITypeDeclSymbol>.CyclicEquals(ITypeDeclSymbol other, ref CyclicEqualityCompareContext context)
            => other is EnumElemDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<EnumElemDeclSymbol>.CyclicEquals(EnumElemDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(EnumElemDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!name.Equals(other.name))
                return false;

            if (!memberVarDecls.CyclicEqualsClassItem(ref other.memberVarDecls, ref context))
                return false;

            if (!initState.Equals(other.initState))
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(outer), outer);
            context.SerializeRef(nameof(name), name);
            context.SerializeRefArray(nameof(memberVarDecls), memberVarDecls);
            context.SerializeString(nameof(initState), initState.ToString());
        }
    }
}