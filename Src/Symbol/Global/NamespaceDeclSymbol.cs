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
    public class NamespaceDeclSymbol : ITopLevelDeclSymbolNode, ITopLevelDeclContainable
    {
        ITopLevelDeclSymbolNode outer;
        Name name;

        TopLevelDeclSymbolComponent topLevelComp;

        public NamespaceDeclSymbol(ITopLevelDeclSymbolNode outer, Name name)
        {
            this.outer = outer;
            this.name = name;
            this.topLevelComp = TopLevelDeclSymbolComponent.Make();
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {   
            return outer;
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
        
        public Accessor GetAccessor()
        {
            return Accessor.Public; // TODO: private으로 지정할 수 있을까
        }

        public ITypeDeclSymbol? GetType(Name name, int typeParamCount)
           => topLevelComp.GetType(name, typeParamCount);

        public GlobalFuncDeclSymbol? GetFunc(Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
            => topLevelComp.GetFunc(name, typeParamCount, paramIds);

        public IEnumerable<GlobalFuncDeclSymbol> GetFuncs(Name name, int minTypeParamCount)
            => topLevelComp.GetFuncs(name, minTypeParamCount);

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
            => topLevelComp.GetMemberDeclNodes();

        public void AddNamespace(NamespaceDeclSymbol declSymbol)
            => topLevelComp.AddNamespace(declSymbol);

        public void AddType(ITypeDeclSymbol typeDeclSymbol)
            => topLevelComp.AddType(typeDeclSymbol);

        public void AddFunc(GlobalFuncDeclSymbol funcDeclSymbol)
            => topLevelComp.AddFunc(funcDeclSymbol);
    }
}
