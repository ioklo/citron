#pragma once
#include <vector>
#include <memory>
#include <expected>

#include "TranslationTasks.h"

namespace Citron {

struct MFuncBody;

using LoggerPtr = std::shared_ptr<class Logger>;
using DiagPtr = std::shared_ptr<struct Diag>;
using MFactoryPtr = std::shared_ptr<class MFactory>;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using NFactoryPtr = std::shared_ptr<class NFactory>;

using SRTFactoryPtr = std::shared_ptr<class SRTFactory>;
using BinOpQueryServicePtr = std::shared_ptr<class BinOpQueryService>;

// Phase 1 : ResolveTypeHierarchyPhase 
// Phase 2 : BuildTypeDependentSymbolPhase 
// Phase 3 : SynthesizeImplicitPhase 
// Phase 4 : TranslateBodyPhase

class PhaseManager
{   
    LoggerPtr logger;
    RFactoryPtr rFactory;
    NFactoryPtr nFactory;
    MFactoryPtr mFactory;

    SRTFactoryPtr srtFactory;
    BinOpQueryServicePtr binOpQueryService;

    std::vector<std::shared_ptr<IResolveTypeHierarchyTask>> resolveTypeHierarchyTasks;
    std::vector<std::shared_ptr<IBuildTypeDependentSymbolTask>> buildTypeDependentSymbolTasks;
    std::vector<std::shared_ptr<ISynthesizeImplicitSymbolTask>> synthesizeImplicitSymbolTask;
    std::vector<std::shared_ptr<ITranslateBodyTask>> translatingBodyTasks;

public:
    PhaseManager(
        const LoggerPtr& logger, 
        const RFactoryPtr& rFactory, const NFactoryPtr& nFactory, const MFactoryPtr& mFactory,
        const SRTFactoryPtr& srtFactory, const BinOpQueryServicePtr& binOpQueryService);
    ~PhaseManager(); 

    void AddResolveTypeHierarchyTask(std::shared_ptr<IResolveTypeHierarchyTask>&& task);
    void AddBuildTypeDependentSymbolTask(std::shared_ptr<IBuildTypeDependentSymbolTask>&& task);
    void AddSynthesizeImplicitSymbolTask(std::shared_ptr<ISynthesizeImplicitSymbolTask>&& task);
    void AddTranslateBodyTask(std::shared_ptr<ITranslateBodyTask>&& task);

    std::expected<std::vector<MFuncBody>, DiagPtr> Run();
};


} // namespace Citron