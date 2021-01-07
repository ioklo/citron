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
        //Phase0 phase0;        

        //public Translator(Phase0 phase0)
        //{
        //    this.phase0 = phase0;
        //}

        public static Script? Translate(IEnumerable<IModuleInfo> referenceInfos, Syntax.Script script, IErrorCollector errorCollector)
        {
            var moduleInfoRepo = new ModuleInfoRepository(referenceInfos);
            var itemInfoRepo = new ItemInfoRepository(moduleInfoRepo);

            var typeSkeletonCollector = new TypeSkeletonCollector();
            
            var phase1Factory = new Phase1Factory(referenceInfos, script, itemInfoRepo, errorCollector);
            var phase0 = new Phase0(script, typeSkeletonCollector, phase1Factory);

            // 1. skeleton을 모은다
            var phase1 = phase0.Run();
            if (phase1 == null) return null;

            var phase2 = phase1.Run();
            if (phase1 == null) return null;

            var phase3 = phase2.Run();

        }
    }
}
