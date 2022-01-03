using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Gum.Infra;
using Gum.Collections;

using R = Gum.IR0;
using M = Gum.CompileTime;
using Gum.Log;
using Gum.Analysis;

namespace Gum.IR0Translator
{
    // 외부 인터페이스
    public class Translator
    {
        public static R.Script? Translate(M.Name moduleName, ImmutableArray<M.ModuleDecl> referenceModules, Syntax.Script sscript, ILogger logger)
        {
            Debug.Assert(!referenceModules.Contains(RuntimeModuleDecl.Instance));

            var externalModuleInfoRepo = new ExternalModuleInfoRepository(referenceModules.Add(RuntimeModuleDecl.Instance));

            // Make syntax based Analyzer (almost translation)
            var rscript = Analyzer.Analyze(sscript, moduleName, externalModuleInfoRepo, logger);
            if (rscript == null) return null;

            if (logger.HasError)
                return null;
            
            // 분석을 하면 결과는 logger에 들어가게 된다
            IR0Analyzer.NullRefAnalysis.ScriptAnalyzer.Analyze(rscript, logger);

            if (logger.HasError)
                return null;

            return rscript;
        }
    }
}
