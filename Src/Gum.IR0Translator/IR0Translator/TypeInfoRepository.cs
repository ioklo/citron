using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;

using M = Gum.CompileTime;
using System.Diagnostics;
using Gum.Infra;

namespace Gum.IR0Translator
{
    // reference module에서 타입 정보를 얻어오는 역할
    class TypeInfoRepository : IPure
    {
        IModuleInfo internalModuleInfo;
        ModuleInfoRepository externalModuleInfoRepo;

        public TypeInfoRepository(IModuleInfo internalModuleInfo, ModuleInfoRepository moduleInfoRepo)
        {
            this.internalModuleInfo = internalModuleInfo;
            this.externalModuleInfoRepo = moduleInfoRepo;
        }

        public void EnsurePure()
        {
            // Check purity
            Infra.Misc.EnsurePure(internalModuleInfo);
            Infra.Misc.EnsurePure(externalModuleInfoRepo);
        }

        public IModuleTypeInfo? GetType(M.ModuleName moduleName, M.NamespacePath namespacePath, M.Name name, int typeParamCount)
        {
            var itemPathEntry = new ItemPathEntry(name, typeParamCount);

            if (internalModuleInfo.GetName().Equals(moduleName))
                return GlobalItemQueryService.GetGlobalItem(internalModuleInfo, namespacePath, itemPathEntry) as IModuleTypeInfo;

            foreach (var module in externalModuleInfoRepo.GetAllModules())
                if (module.GetName().Equals(moduleName))
                    return GlobalItemQueryService.GetGlobalItem(module, namespacePath, itemPathEntry) as IModuleTypeInfo;

            return null;
        }
    }
}
