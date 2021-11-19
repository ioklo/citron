using Gum.Analysis;
using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [ImplementIEquatable]
    public partial class ExternalModuleInfo : IModuleInfo
    {
        M.ModuleName name;
        ExternalNamespaceDict namespaceDict;
        ModuleTypeDict typeDict;
        ModuleFuncDict funcDict;

        void IPure.EnsurePure()
        {
            throw new NotImplementedException();
        }

        public ExternalModuleInfo(M.ModuleInfo moduleInfo)
        {
            name = moduleInfo.Name;

            namespaceDict = new ExternalNamespaceDict(moduleInfo.Namespaces);
            typeDict = new ModuleTypeDict(moduleInfo.Types);
            funcDict = new ModuleFuncDict(moduleInfo.Funcs);
        }

        public M.ModuleName GetName()
        {
            return name;
        }

        M.ModuleName IModuleInfo.GetName()
        {
            return name;
        }

        IModuleNamespaceInfo? IModuleNamespaceContainer.GetNamespace(M.NamespaceName name)
        {
            return namespaceDict.Get(name);
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            return typeDict.Get(name, typeParamCount);
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            return funcDict.Get(name, minTypeParamCount);
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return funcDict.Get(name, typeParamCount, paramTypes);
        }        
    }
}
