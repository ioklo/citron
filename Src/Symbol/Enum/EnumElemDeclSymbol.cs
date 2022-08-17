using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using Citron.Module;

namespace Citron.Symbol
{
    [AutoConstructor, ImplementIEquatable]
    public partial class EnumElemDeclSymbol : ITypeDeclSymbol
    {
        IHolder<EnumDeclSymbol> outer;
        Name name;
        ImmutableArray<EnumElemMemberVarDeclSymbol> memberVarDecls;
        
        public int GetMemberVarCount()
        {
            return memberVarDecls.Length;
        }

        public EnumElemMemberVarDeclSymbol GetMemberVar(int index)
        {
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
            return outer.GetValue();
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
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

        public AccessModifier GetAccessModifier()
        {
            return AccessModifier.Public;
        }
    }
}