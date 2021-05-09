using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;

using M = Gum.CompileTime;
using System.Diagnostics;

namespace Gum.IR0Translator
{
    // reference module에서 타입 정보를 얻어오는 역할
    class TypeInfoRepository : ICloneable<TypeInfoRepository>
    {
        M.ModuleInfo internalModuleInfo;
        ModuleInfoRepository externalModuleInfoRepo;

        public TypeInfoRepository(M.ModuleInfo internalModuleInfo, ModuleInfoRepository moduleInfoRepo)
        {
            this.internalModuleInfo = internalModuleInfo;
            this.externalModuleInfoRepo = moduleInfoRepo;
        }        
        
        public TypeInfoRepository Clone(CloneContext context)
        {
            Debug.Assert(Infra.Misc.EnsurePure(internalModuleInfo));
            Debug.Assert(Infra.Misc.EnsurePure(externalModuleInfoRepo));

            return this;
        }

        public M.TypeInfo? GetType(M.ModuleName moduleName, M.NamespacePath namespacePath, M.Name name, int typeParamCount)
        {
            var itemPathEntry = new ItemPathEntry(name, typeParamCount);

            if (internalModuleInfo.Name.Equals(moduleName))
                return GlobalItemQueryService.GetGlobalItem(internalModuleInfo, namespacePath, itemPathEntry) as M.TypeInfo;

            foreach (var module in externalModuleInfoRepo.GetAllModules())
                if (module.Name.Equals(moduleName))
                    return GlobalItemQueryService.GetGlobalItem(module, namespacePath, itemPathEntry) as M.TypeInfo;

            return null;
        }
    }
}
