﻿using System;
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
        public static R.Script? Translate(M.ModuleName moduleName, ImmutableArray<M.ModuleInfo> referenceInfos, Syntax.Script sscript, ILogger logger)
        {
            Debug.Assert(!referenceInfos.Contains(RuntimeModuleInfo.Instance));

            var externalModuleInfoRepo = new ExternalModuleInfoRepository(referenceInfos.Add(RuntimeModuleInfo.Instance));

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
