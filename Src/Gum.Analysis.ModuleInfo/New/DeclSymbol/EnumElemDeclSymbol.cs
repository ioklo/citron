using Gum.Analysis;
using Gum.Collections;
using Pretune;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor, ImplementIEquatable]
    public partial class EnumElemDeclSymbol : ITypeDeclSymbolNode
    {
        Lazy<EnumDeclSymbol> outer;
        M.Name name;
        ImmutableArray<EnumElemMemberVarDeclSymbol> memberVarDecls;

        public ImmutableArray<EnumElemMemberVarDeclSymbol> GetMemberVarDecls()
        {
            return memberVarDecls;
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
            return outer.Value;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            if (typeParamCount != 0 || !paramTypes.IsEmpty) return null;

            foreach(var decl in memberVarDecls)
            {
                if (decl.GetName().Equals(name))
                    return decl;
            }

            return null;
        }

        public void Apply(ITypeDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitEnumElemDecl(this);
        }
    }
}