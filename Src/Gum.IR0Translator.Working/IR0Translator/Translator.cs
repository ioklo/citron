using System;
using System.Collections.Generic;
using System.Text;
using Gum.IR0;
using Gum.Infra;
using Gum.CompileTime;
using System.Collections.Immutable;

namespace Gum.IR0Translator
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

            var typeInfoRepo = new TypeInfoRepository(internalModuleInfo, externalModuleInfoRepo);
            var ritemFactory = new IR0ItemFactory();
            var itemValueFactory = new ItemValueFactory(typeInfoRepo, ritemFactory);
            var globalItemValueFactory = new GlobalItemValueFactory(itemValueFactory, internalModuleInfo, externalModuleInfoRepo);

            // Make Analyzer
            var script = Analyzer.Analyze(sscript, itemValueFactory, globalItemValueFactory, typeExpTypeValueService, errorCollector);

            return script;
        }
    }
}
