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

namespace Gum.Analysis
{
    // 'Translation 단계에서만' 사용하는 레퍼런스 검색 (TypeExpEvaluator, Analyzer에서 사용한다)
    public class ExternalModuleInfoRepository : IPure
    {
        ImmutableArray<ExternalModuleInfo> moduleInfos;

        public ExternalModuleInfoRepository(ImmutableArray<ModuleInfo> moduleInfos)
        {
            var builder = ImmutableArray.CreateBuilder<ExternalModuleInfo>(moduleInfos.Length);

            foreach (var moduleInfo in moduleInfos)
                builder.Add(new ExternalModuleInfo(moduleInfo));

            this.moduleInfos = builder.MoveToImmutable();
        }

        public void EnsurePure()
        {   
        }

        public ImmutableArray<ExternalModuleInfo> GetAllModules()
        {
            return moduleInfos;
        }
    }
}
