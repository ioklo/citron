using Gum.CompileTime;
using Gum.Infra;
using S = Gum.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

namespace Gum.IR0
{
    class Phase2Factory
    {
        ImmutableArray<IModuleInfo> referenceInfos;
        IErrorCollector errorCollector;

        public Phase2Factory(ImmutableArray<IModuleInfo> referenceInfos, IErrorCollector errorCollector)
        {
            this.referenceInfos = referenceInfos;
            this.errorCollector = errorCollector;
        }

        public Phase2 Make(SkeletonRepository skelRepo, TypeExpTypeValueService typeExpTypeValueService)
        {            
            var moduleInfoBuilder = new ModuleInfoBuilder(typeExpTypeValueService);
            var syntaxItemInfoRepoBuilder = new SyntaxItemInfoRepoBuilder();
            var phase3Factory = new Phase3Factory(referenceInfos, errorCollector, typeExpTypeValueService);

            return new Phase2(skelRepo, moduleInfoBuilder, syntaxItemInfoRepoBuilder, phase3Factory);
        }
    }
}