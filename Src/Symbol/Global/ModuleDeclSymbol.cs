using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citron.Collections;
using Citron.Module;

namespace Citron.Symbol
{   
    public class ModuleDeclSymbol : ITopLevelDeclSymbolNode, ITopLevelDeclContainable
    {
        Name moduleName;
        bool bReference;

        TopLevelDeclSymbolComponent topLevelComp;

        public ModuleDeclSymbol(Name moduleName, bool bReference)
        {
            this.moduleName = moduleName;
            this.bReference = bReference;

            this.topLevelComp = TopLevelDeclSymbolComponent.Make();
        }

        public Name GetName()
        {
            return moduleName;
        }        

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(moduleName, 0, default);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return null;
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitModule(this);
        }

        public Accessor GetAccessor()
        {
            return Accessor.Public;
        }

        public bool IsReference()
        {
            return bReference;
        }

        public NamespaceDeclSymbol? GetNamespace(Name name)
            => topLevelComp.GetNamespace(name);

        public ITypeDeclSymbol? GetType(Name name, int typeParamCount)
            => topLevelComp.GetType(name, typeParamCount);

        public GlobalFuncDeclSymbol? GetFunc(Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
            => topLevelComp.GetFunc(name, typeParamCount, paramIds);

        public IEnumerable<GlobalFuncDeclSymbol> GetFuncs(Name name, int minTypeParamCount)
            => topLevelComp.GetFuncs(name, minTypeParamCount);

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
            => topLevelComp.GetMemberDeclNodes();

        public void AddType(ITypeDeclSymbol type)
            => topLevelComp.AddType(type);

        public void AddFunc(GlobalFuncDeclSymbol func)
            => topLevelComp.AddFunc(func);

        public void AddNamespace(NamespaceDeclSymbol ns)
            => topLevelComp.AddNamespace(ns);
    }
}
