using Gum.CompileTime;
using Gum.Infra;
using System.Collections.Generic;
using System.Collections.Immutable;
using S = Gum.Syntax;

namespace Gum.IR0
{
    class Phase1Factory
    {
        ImmutableArray<IModuleInfo> referenceInfos;
        S.Script script;
        ItemInfoRepository itemInfoRepo;
        IErrorCollector errorCollector;        

        public Phase1Factory(IEnumerable<IModuleInfo> referenceInfos, S.Script script, ItemInfoRepository itemInfoRepo, IErrorCollector errorCollector)
        {
            this.referenceInfos = referenceInfos.ToImmutableArray();
            this.script = script;
            this.itemInfoRepo = itemInfoRepo;
            this.errorCollector = errorCollector;
        }

        public Phase1 Make(SkeletonRepository skelRepo)
        {
            var typeExpEvaluator = new TypeExpEvaluator(itemInfoRepo, skelRepo);
            var phase2Factory = new Phase2Factory(referenceInfos, errorCollector);

            return new Phase1(script, typeExpEvaluator, skelRepo, errorCollector, phase2Factory);
        }
    }
}