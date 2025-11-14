#pragma once
#include <vector>
#include <memory>

#include "RSymbol/RFactory.h"
#include "NSymbol/NFactory.h"

#include "TranslationTasks.h"

namespace Citron {
namespace SyntaxIR0Translator {

// Phase 1: BuildTypeSymbolPhase 
// Phase 2 : ResolveTypeHierarchyPhase 
// Phase 3 : BuildTypeDependentSymbolPhase 
// Phase 4 : SynthesizeImplicitPhase 
// Phase 5 : TranslateBodyPhase

class PhaseManager
{
    RFactoryPtr rFactory;
    NFactoryPtr nFactory;

    std::vector<std::shared_ptr<IBuildTypeSymbolTask>> buildTypeSymbolTasks;
    std::vector<std::shared_ptr<IResolveTypeHierarchyTask>> resolveTypeHierarchyTasks;
    std::vector<std::shared_ptr<IBuildTypeDependentSymbolTask>> buildTypeDependentSymbolTasks;
    std::vector<std::shared_ptr<ISynthesizeImplicitSymbolTask>> synthesizeImplicitSymbolTask;
    std::vector<std::shared_ptr<ITranslateBodyTask>> translatingBodyTasks;

public:
    PhaseManager();
    ~PhaseManager(); 

    void AddBuildTypeSymbolTask(std::shared_ptr<IBuildTypeSymbolTask>&& task);
    void AddResolveTypeHierarchyTask(std::shared_ptr<IResolveTypeHierarchyTask>&& task);
    void AddBuildTypeDependentSymbolTask(std::shared_ptr<IBuildTypeDependentSymbolTask>&& task);
    void AddSynthesizeImplicitSymbolTask(std::shared_ptr<ISynthesizeImplicitSymbolTask>&& task);
    void AddTranslateBodyTask(std::shared_ptr<ITranslateBodyTask>&& task);

    void Run();
};


} // namespace SyntaxIR0Translator 
} // namespace Citron