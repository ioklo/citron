using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gum.Collections;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public class ModuleDeclSymbol : ITopLevelDeclSymbolNode
    {
        M.Name moduleName;
        TopLevelDeclDict dict;        

        public ModuleDeclSymbol(M.Name moduleName, ImmutableArray<NamespaceDeclSymbol> namespaces, ImmutableArray<GlobalTypeDeclSymbol> types, ImmutableArray<GlobalFuncDeclSymbol> funcs)
        {
            this.moduleName = moduleName;
            this.dict = new TopLevelDeclDict(namespaces, types, funcs);
        }

        public M.Name GetName()
        {
            return moduleName;
        }        

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(moduleName, 0, default);
        }

        public ImmutableArray<GlobalFuncDeclSymbol> GetFuncs(M.Name name, int minTypeParamCount)
        {
            return dict.GetFuncs(name, minTypeParamCount);
        }
        
        public GlobalTypeDeclSymbol? GetType(M.Name name, int typeParamCount)
        {
            return dict.GetType(name, typeParamCount);
        }

        public GlobalFuncDeclSymbol? GetFunc(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return dict.GetFunc(name, typeParamCount, paramIds);
        }

        public NamespaceDeclSymbol? GetNamespace(M.Name name)
        {
            return null;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return null;
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            if (paramIds.IsEmpty)
            {
                var globalType = GetType(name, typeParamCount);
                if (globalType != null)
                    return globalType.GetNode();
            }

            var globalFunc = GetFunc(name, typeParamCount, paramIds);
            if (globalFunc != null)
                return globalFunc;

            return null;
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitModule(this);
        }

        public M.AccessModifier GetAccessModifier()
        {
            return M.AccessModifier.Public;
        }
    }
}
