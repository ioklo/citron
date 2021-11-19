using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;

using M = Gum.CompileTime;
using System.Diagnostics;
using Gum.Infra;
using Gum.Analysis;

namespace Gum.Analysis
{
    // reference module에서 타입 정보를 얻어오는 역할
    public class TypeInfoRepository : IPure
    {
        IModuleInfo internalInfo;
        ImmutableArray<IModuleInfo> externalInfos;

        public TypeInfoRepository(IModuleInfo internalInfo, ImmutableArray<IModuleInfo> externalInfos)
        {
            this.internalInfo = internalInfo;
            this.externalInfos = externalInfos;
        }

        public void EnsurePure()
        {
            // Check purity
            Infra.Misc.EnsurePure(internalInfo);
            Infra.Misc.EnsurePure(externalInfos);
        }

        public IModuleTypeInfo? GetType(M.ModuleName moduleName, M.NamespacePath namespacePath, M.Name name, int typeParamCount)
        {
            var itemPathEntry = new ItemPathEntry(name, typeParamCount);

            if (internalInfo.GetName().Equals(moduleName))
                return GlobalItemQueryService.GetGlobalItem(internalInfo, namespacePath, itemPathEntry) as IModuleTypeInfo;

            foreach (var externalInfo in externalInfos)
                if (externalInfo.GetName().Equals(moduleName))
                    return GlobalItemQueryService.GetGlobalItem(externalInfo, namespacePath, itemPathEntry) as IModuleTypeInfo;

            return null;
        }
    }
}
