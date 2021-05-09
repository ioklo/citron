using Gum.CompileTime;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Gum.Infra;

namespace Gum.IR0Translator
{
    // 'Translation 단계에서만' 사용하는 레퍼런스 검색 (TypeExpEvaluator, Analyzer에서 사용한다)
    class ModuleInfoRepository : IPure
    {
        ImmutableArray<ModuleInfo> moduleInfos;

        public ModuleInfoRepository(ImmutableArray<ModuleInfo> moduleInfos)
        {
            this.moduleInfos = moduleInfos;
        }

        public void EnsurePure()
        {   
        }

        public ImmutableArray<ModuleInfo> GetAllModules()
        {
            return moduleInfos;
        }
    }
}
