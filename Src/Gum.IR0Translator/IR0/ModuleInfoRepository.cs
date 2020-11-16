using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Gum.IR0
{
    // 'Translation 단계에서만' 사용하는 레퍼런스 검색 (TypeExpEvaluator, Analyzer에서 사용한다)
    public class ModuleInfoRepository
    {
        ImmutableDictionary<ModuleName, IModuleInfo> modulesByName;
        ImmutableArray<IModuleInfo> moduleInfos;

        // ScriptModuleInfo는 여기서 검색하지 않는다        
        public IEnumerable<IModuleInfo> GetAllModules()
        {
            return moduleInfos;
        }

        public ModuleInfoRepository(IEnumerable<IModuleInfo> moduleInfos)
        {
            this.moduleInfos = moduleInfos.ToImmutableArray();
            modulesByName = this.moduleInfos.ToImmutableDictionary(moduleInfo => moduleInfo.GetModuleName());
        }

        public IModuleInfo? GetModule(ModuleName moduleName)
        {
            return modulesByName.GetValueOrDefault(moduleName);
        }
    }
}
