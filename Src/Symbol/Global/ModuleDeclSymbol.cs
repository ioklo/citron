using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citron.Collections;
using Citron.Module;

namespace Citron.Symbol
{
    public class ModuleDeclSymbol : ITopLevelDeclSymbolNode
    {
        Name moduleName;
        TopLevelDeclDict dict;

        public ModuleDeclSymbol(Name moduleName, ImmutableArray<NamespaceDeclSymbol> namespaces, ImmutableArray<GlobalTypeDeclSymbol> types, ImmutableArray<GlobalFuncDeclSymbol> funcs)
        {
            this.moduleName = moduleName;
            this.dict = new TopLevelDeclDict(namespaces, types, funcs);
        }

        public Name GetName()
        {
            return moduleName;
        }        

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(moduleName, 0, default);
        }

        public ImmutableArray<GlobalFuncDeclSymbol> GetFuncs(Name name, int minTypeParamCount)
        {
            return dict.GetFuncs(name, minTypeParamCount);
        }
        
        public GlobalTypeDeclSymbol? GetType(Name name, int typeParamCount)
        {
            return dict.GetType(name, typeParamCount);
        }

        public GlobalFuncDeclSymbol? GetFunc(Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return dict.GetFunc(name, typeParamCount, paramIds);
        }

        public NamespaceDeclSymbol? GetNamespace(Name name)
        {
            return null;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return null;
        }
        
        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return dict.GetMemberDeclNodes();
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitModule(this);
        }

        public AccessModifier GetAccessModifier()
        {
            return AccessModifier.Public;
        }
    }
}
