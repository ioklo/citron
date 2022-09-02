using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Symbol
{
    public class NamespaceDeclSymbol : ITopLevelDeclSymbolNode
    {
        IHolder<ITopLevelDeclSymbolNode> outer;
        Name name;
        TopLevelDeclDict dict;

        public NamespaceDeclSymbol(IHolder<ITopLevelDeclSymbolNode> outer, Name name, ImmutableArray<NamespaceDeclSymbol> namespaces, ImmutableArray<ITypeDeclSymbol> types, ImmutableArray<GlobalFuncDeclSymbol> funcs)
        {
            this.outer = outer;
            this.name = name;
            this.dict = new TopLevelDeclDict(namespaces, types, funcs);
        }
        
        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return dict.GetMemberDeclNodes();
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public Name GetName()
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

        public ITypeDeclSymbol? GetType(Name name, int typeParamCount)
        {
            return dict.GetType(name, typeParamCount);
        }

        public GlobalFuncDeclSymbol? GetFunc(Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return dict.GetFunc(name, typeParamCount, paramIds);
        }

        public ImmutableArray<GlobalFuncDeclSymbol> GetFuncs(Name name, int minTypeParamCount)
        {
            return dict.GetFuncs(name, minTypeParamCount);
        }

        public AccessModifier GetAccessModifier()
        {
            return AccessModifier.Public; // TODO: private으로 지정할 수 있을까
        }
    }
}
