using System;
using System.Collections.Generic;
using System.Text;
using Gum.IR0;
using Gum.Infra;
using Gum.CompileTime;
using System.Collections.Immutable;

namespace Gum.IR0
{
    // 외부 인터페이스
    public class Translator
    {
        public static Script? Translate(ModuleName moduleName, ImmutableArray<ModuleInfo> referenceInfos, Syntax.Script sscript, IErrorCollector errorCollector)
        {
            var externalModuleInfoRepo = new ModuleInfoRepository(referenceInfos);

            var typeSkelRepo = TypeSkeletonCollector.Collect(sscript);
            var typeExpTypeValueService = TypeExpEvaluator.Evaluate(moduleName, sscript, externalModuleInfoRepo, typeSkelRepo, errorCollector);

            var internalModuleInfo = ModuleInfoBuilder.Build(moduleName, sscript, typeExpTypeValueService);

            // Make Analyzer
            var script = Analyzer.Analyze(sscript, typeExpTypeValueService, errorCollector);

            return script;
        }
    }
}
