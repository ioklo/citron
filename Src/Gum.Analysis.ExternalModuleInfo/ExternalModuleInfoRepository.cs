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
using System.Diagnostics.Contracts;

namespace Gum.Analysis
{
    // 'Translation 단계에서만' 사용하는 레퍼런스 검색 (TypeExpEvaluator, Analyzer에서 사용한다)
    [Pure]
    public class ExternalModuleInfoRepository 
    {
        ImmutableArray<ExternalModuleDecl> moduleInfos;

        public ExternalModuleInfoRepository(ImmutableArray<M.ModuleDecl> moduleInfos)
        {
            var builder = ImmutableArray.CreateBuilder<ExternalModuleDecl>(moduleInfos.Length);

            foreach (var moduleInfo in moduleInfos)
                builder.Add(new ExternalModuleDecl(moduleInfo));

            this.moduleInfos = builder.MoveToImmutable();
        }

        public ImmutableArray<ExternalModuleDecl> GetAllModules()
        {
            return moduleInfos;
        }
    }
}
