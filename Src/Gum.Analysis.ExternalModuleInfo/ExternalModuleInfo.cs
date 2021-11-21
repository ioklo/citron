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

            typeDict = ExternalModuleMisc.MakeModuleTypeDict(moduleInfo.Types);            
            funcDict = ExternalModuleMisc.MakeModuleFuncDict(moduleInfo.Funcs);
        }

        public M.ModuleName GetName()
        {
            return name;
        }

        M.ModuleName IModuleInfo.GetName()
        {
            return name;
        }

        IModuleNamespaceInfo? IModuleNamespaceContainer.GetNamespace(M.Name name)
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

        IModuleItemInfo? IModuleInfo.GetItem(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            var candidates = new Candidates<IModuleItemInfo>();

            if (typeParamCount == 0 && paramTypes.IsEmpty)
            {
                var ns = namespaceDict.Get(name);
                if (ns != null)
                    candidates.Add(ns);
            }

            if (paramTypes.IsEmpty)
            {
                var type = typeDict.Get(name, typeParamCount);
                if (type != null)
                    candidates.Add(type);
            }

            var func = funcDict.Get(name, typeParamCount, paramTypes);
            if (func != null)
                candidates.Add(func);

            return candidates.GetSingle(); // TODO: 에러처리
        }
    }
}
