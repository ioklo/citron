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
        public static Script? Translate(IEnumerable<IModuleInfo> referenceInfos, Syntax.Script script, IErrorCollector errorCollector)
        {
            var externalModuleInfoRepo = new ModuleInfoRepository(referenceInfos);

            var typeSkelRepo = TypeSkeletonCollector.Collect(script);
            var typeExpTypeValueService = TypeExpEvaluator.Evaluate(script, externalModuleInfoRepo, typeSkelRepo, errorCollector);

            var internalModuleInfo = ModuleInfoBuilder.Build(script, typeExpTypeValueService);

            // Make Analyzer
            var itemInfoRepo = new ItemInfoRepository(internalModuleInfo, externalModuleInfoRepo);
            var typeValueApplier = new TypeValueApplier(itemInfoRepo);
            var typeValueService = new TypeValueService(itemInfoRepo, typeValueApplier);

            var analyzer = new Analyzer(typeSkelRepo, itemInfoRepo, typeValueService, typeExpTypeValueService, errorCollector);

            return analyzer.AnalyzeScript(script, errorCollector);            
        }
    }
}
