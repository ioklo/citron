#pragma once
#include <vector>
#include <memory>
#include <expected>
#include "Infra/Ref.h"
#include "TranslationTasks.h"

namespace Citron {

class MFuncBody;

using LoggerPtr = std::shared_ptr<class Logger>;
using DiagPtr = std::shared_ptr<struct Diag>;
using MFactoryPtr = std::shared_ptr<class MFactory>;
using RFactoryPtr = std::shared_ptr<class RFactory>;

using SRTFactoryPtr = std::shared_ptr<class SRTFactory>;
using BinOpQueryServicePtr = std::shared_ptr<class BinOpQueryService>;

// Phase 1 : BuildTypeSymbolPhase (body-space에서 만들어지는 lambda 제외)
// Phase 2 : BuildTypeHierarchyPhase (inheritance)
// Phase 3 : BuildNonTypeSymbolPhase (func, var)
//           PostBuildNonTypeSymbolPhase (impl ...)
// Phase 4 : SynthesizeImplicitPhase (memberwise-ctor)
// Phase 5 : TranslateBodyPhase 

class PhaseManager
{   
    LoggerPtr logger;
    RFactoryPtr rFactory;
    MFactoryPtr mFactory;

    SRTFactoryPtr srtFactory;
    BinOpQueryServicePtr binOpQueryService;

    std::vector<std::shared_ptr<IBuildTypeHierarchyTask>> buildTypeHierarchyTasks;
    std::vector<std::shared_ptr<IBuildNonTypeSymbolTask>> buildNonTypeSymbolTasks;
    std::vector<std::shared_ptr<IPostBuildNonTypeSymbolTask>> postBuildNonTypeSymbolTasks;
    std::vector<std::shared_ptr<IBuildImplicitSymbolTask>> buildImplicitSymbolTasks;
    std::vector<std::shared_ptr<ITranslateBodyTask>> translatingBodyTasks;

public:
    PhaseManager(
        TakeRef<LoggerPtr> logger, 
        TakeRef<RFactoryPtr> rFactory, TakeRef<MFactoryPtr> mFactory,
        TakeRef<SRTFactoryPtr> srtFactory, TakeRef<BinOpQueryServicePtr> binOpQueryService);
    ~PhaseManager(); 

    void AddBuildTypeHierarchyTask(std::shared_ptr<IBuildTypeHierarchyTask>&& task);
    void AddBuildNonTypeSymbolTask(std::shared_ptr<IBuildNonTypeSymbolTask>&& task);
    void AddPostBuildNonTypeSymbolTask(std::shared_ptr<IPostBuildNonTypeSymbolTask>&& task);
    void AddBuildImplicitSymbolTask(std::shared_ptr<IBuildImplicitSymbolTask>&& task);
    void AddTranslateBodyTask(std::shared_ptr<ITranslateBodyTask>&& task);

    std::expected<std::vector<MFuncBody>, DiagPtr> Run();
};


} // namespace Citron