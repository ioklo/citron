using Gum.CompileTime;
using Gum.Infra;
using System;
using System.Collections.Generic;
using S = Gum.Syntax;

namespace Gum.IR0
{
    // ModuleInfoBuilder
    class Phase2
    {
        TypeSkeletonRepository skelRepo;
        ModuleInfoBuilder moduleInfoBuilder;

        // for making analyzer        
        ModuleInfoRepository externalModuleInfoRepo;
        IErrorCollector errorCollector;
        TypeExpTypeValueService typeExpTypeValueService;

        public Phase2(
            TypeSkeletonRepository skelRepo, ModuleInfoBuilder moduleInfoBuilder,
            ModuleInfoRepository externalModuleInfoRepo, IErrorCollector errorCollector, TypeExpTypeValueService typeExpTypeValueService)
        {
            this.skelRepo = skelRepo;
            this.moduleInfoBuilder = moduleInfoBuilder;
            
            this.externalModuleInfoRepo = externalModuleInfoRepo;
            this.errorCollector = errorCollector;
            this.typeExpTypeValueService = typeExpTypeValueService;
        }

        
    }
}