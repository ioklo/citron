using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Citron.CompileTime;

namespace Citron.Analysis
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

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<M.FuncParamId> paramIds)
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

        public GlobalTypeDeclSymbol? GetType(M.Name name, int typeParamCount)
        {
            return dict.GetType(name, typeParamCount);
        }

        public GlobalFuncDeclSymbol? GetFunc(M.Name name, int typeParamCount, ImmutableArray<M.FuncParamId> paramIds)
        {
            return dict.GetFunc(name, typeParamCount, paramIds);
        }

        public ImmutableArray<GlobalFuncDeclSymbol> GetFuncs(M.Name name, int minTypeParamCount)
        {
            return dict.GetFuncs(name, minTypeParamCount);
        }

        public M.AccessModifier GetAccessModifier()
        {
            return M.AccessModifier.Public; // TODO: private으로 지정할 수 있을까
        }
    }
}
