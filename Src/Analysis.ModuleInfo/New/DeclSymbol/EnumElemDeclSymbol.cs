using Citron.Analysis;
using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using M = Citron.CompileTime;

namespace Citron.Analysis
{
    [AutoConstructor, ImplementIEquatable]
    public partial class EnumElemDeclSymbol : ITypeDeclSymbol
    {
        IHolder<EnumDeclSymbol> outer;
        M.Name name;
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

        public M.Name GetName()
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

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<M.FuncParamId> paramIds)
        {
            if (typeParamCount != 0 || !paramIds.IsEmpty) return null;

            foreach(var decl in memberVarDecls)
            {
                if (decl.GetName().Equals(name))
                    return decl;
            }

            return null;
        }

        public void Apply(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitEnumElem(this);
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitEnumElem(this);
        }

        public M.AccessModifier GetAccessModifier()
        {
            return M.AccessModifier.Public;
        }
    }
}