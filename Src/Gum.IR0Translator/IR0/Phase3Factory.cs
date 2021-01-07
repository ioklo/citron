using Gum.CompileTime;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Gum.IR0
{
    class Phase3Factory
    {
        ImmutableArray<IModuleInfo> referenceInfos;
        IErrorCollector errorCollector;
        TypeExpTypeValueService typeExpTypeValueService;

        // ItemInfoRepository가 왜 필요한가 Item찾으려고 => 필요한 곳에 다 달아놓으면?
        public Phase3Factory(
            ImmutableArray<IModuleInfo> referenceInfos, 
            IErrorCollector errorCollector,
            TypeExpTypeValueService typeExpTypeValueService)
        {
            this.referenceInfos = referenceInfos;
            this.errorCollector = errorCollector;
            this.typeExpTypeValueService = typeExpTypeValueService;
        }

        public Phase3 Make(ScriptModuleInfo scriptModuleInfo, SyntaxItemInfoRepository syntaxItemInfoRepo)
        {
            var moduleInfoRepo = new ModuleInfoRepository(referenceInfos.Append(scriptModuleInfo));
            var itemInfoRepo = new ItemInfoRepository(moduleInfoRepo);
            var typeValueApplier = new TypeValueApplier(moduleInfoRepo);
            var typeValueService = new TypeValueService(itemInfoRepo, typeValueApplier);

            var context = new Analyzer.Context(
                itemInfoRepo, syntaxItemInfoRepo, typeValueService, typeExpTypeValueService, errorCollector);

            var analyzer = new Analyzer(context);

            return new Phase3(analyzer);
        }
    }
}
