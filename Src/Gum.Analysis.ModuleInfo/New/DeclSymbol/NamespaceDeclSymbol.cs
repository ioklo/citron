using Gum.Collections;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public class NamespaceDeclSymbol : ITopLevelDeclSymbolNode
    {
        IHolder<ITopLevelDeclSymbolNode> outer;
        M.Name name;
        TopLevelDeclDict dict;

        public NamespaceDeclSymbol(IHolder<ITopLevelDeclSymbolNode> outer, M.Name name, ImmutableArray<NamespaceDeclSymbol> namespaces, ImmutableArray<GlobalTypeDeclSymbol> types, ImmutableArray<GlobalFuncDeclSymbol> funcs)
        {
            this.outer = outer;
            this.name = name;
            this.dict = new TopLevelDeclDict(namespaces, types, funcs);
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return dict.GetMemberDeclNode(name, typeParamCount, paramIds);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public M.Name GetName()
        {
            return name;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitNamespace(this);
        }
    }
}
