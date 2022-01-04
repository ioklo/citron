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

            var referenceModuleSymbols = ModuleDeclSymbolBuilder.Build(referenceModules);

            // Make syntax based Analyzer (almost translation), 결과는 ir0가 아니라 ir0와 정보를 갖고 있는 트리, 나중에 ir0로 export할 수 있다
            var rscript = Analyzer.Analyze(sscript, moduleName, referenceModuleSymbols, logger);
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
