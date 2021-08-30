using System;
using System.Collections.Generic;
using System.Text;
using Gum.Infra;
using Gum.Collections;

using R = Gum.IR0;
using M = Gum.CompileTime;
using System.Diagnostics;

namespace Gum.IR0Translator
{
    // 외부 인터페이스
    public class Translator
    {
        public static R.Script? Translate(M.ModuleName moduleName, ImmutableArray<M.ModuleInfo> referenceInfos, Syntax.Script sscript, IErrorCollector errorCollector)
        {
            Debug.Assert(!referenceInfos.Contains(RuntimeModuleInfo.Instance));

            var externalModuleInfoRepo = new ModuleInfoRepository(referenceInfos.Add(RuntimeModuleInfo.Instance));

            var typeSkelRepo = TypeSkeletonCollector.Collect(sscript);
            var typeExpInfoService = TypeExpEvaluator.Evaluate(moduleName, sscript, externalModuleInfoRepo, typeSkelRepo, errorCollector);

            // Make Analyzer
            var script = Analyzer.Analyze(sscript, moduleName, typeExpInfoService, externalModuleInfoRepo, errorCollector);
            if (script == null) return null;

            if (errorCollector.HasError)
                return null;

            return script;
        }
    }
}
