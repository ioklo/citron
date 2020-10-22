using System;
using System.Collections.Generic;
using System.Text;
using Gum.IR0;
using Gum.Infra;
using Gum.CompileTime;

namespace Gum.IR0
{
    // 외부 인터페이스
    public class IR0Translator
    {
        Analyzer analyzer;
        public IR0Translator()
        {
            var typeSkeletonCollector = new TypeSkeletonCollector();
            var typeExpEvaluator = new TypeExpEvaluator(typeSkeletonCollector);
            var moduleInfoBuilder = new ModuleInfoBuilder(typeExpEvaluator);
            var capturer = new Capturer();

            analyzer = new Analyzer(moduleInfoBuilder, capturer);
        }

        public IR0.Script? Translate(string moduleName, Syntax.Script script, IEnumerable<IModuleInfo> moduleInfos, IErrorCollector errorCollector)
        {   
            var optionalAnalyzeResult = analyzer.AnalyzeScript(moduleName, script, moduleInfos, errorCollector);
            if (optionalAnalyzeResult == null)
                return null;

            return optionalAnalyzeResult.Value.Script;
        }
    }
}
