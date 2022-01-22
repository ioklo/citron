using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    class TupleMemberVarDeclSymbol : IDeclSymbolNode
    {
        ITypeSymbol declType;
        string? name;
        int index;

        public ITypeSymbol GetDeclType()
        {
            return declType;
        }

        public string GetName()
        {
            if (name == null)
                return $"Item{index}";

            return name;
        }
    }

    class TupleDeclSymbol : ITypeDeclSymbol
    {
        ImmutableArray<TupleMemberVarDeclSymbol> memberVars;

        public void Apply(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitTuple(this);
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitTuple(this);
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            if (typeParamCount != 0 || !paramIds.IsEmpty)
                return null;

            foreach(var memberVar in memberVars)
            {
                if (name.Equals(memberVar.GetName()))
                    return memberVar;
            }

            return null;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            // name은 유니크한데 이걸로 찾을 수 있을까
            return new DeclSymbolNodeName();
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return null;
        }
    }
}
