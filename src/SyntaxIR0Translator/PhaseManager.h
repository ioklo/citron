#pragma once
#include <vector>
#include <memory>

#include "RSymbol/RFactory.h"
#include "NSymbol/NFactory.h"

#include "TranslationTasks.h"

namespace Citron {
namespace SyntaxIR0Translator {

// Phase 1 : ResolveTypeHierarchyPhase 
// Phase 2 : BuildTypeDependentSymbolPhase 
// Phase 3 : SynthesizeImplicitPhase 
// Phase 4 : TranslateBodyPhase

class PhaseManager
{
    RFactoryPtr rFactory;
    NFactoryPtr nFactory;

    std::vector<std::shared_ptr<IResolveTypeHierarchyTask>> resolveTypeHierarchyTasks;
    std::vector<std::shared_ptr<IBuildTypeDependentSymbolTask>> buildTypeDependentSymbolTasks;
    std::vector<std::shared_ptr<ISynthesizeImplicitSymbolTask>> synthesizeImplicitSymbolTask;
    std::vector<std::shared_ptr<ITranslateBodyTask>> translatingBodyTasks;

public:
    PhaseManager(const RFactoryPtr& rFactory, const NFactoryPtr& nFactory);
    ~PhaseManager(); 

    void AddResolveTypeHierarchyTask(std::shared_ptr<IResolveTypeHierarchyTask>&& task);
    void AddBuildTypeDependentSymbolTask(std::shared_ptr<IBuildTypeDependentSymbolTask>&& task);
    void AddSynthesizeImplicitSymbolTask(std::shared_ptr<ISynthesizeImplicitSymbolTask>&& task);
    void AddTranslateBodyTask(std::shared_ptr<ITranslateBodyTask>&& task);

    void Run();
};


} // namespace SyntaxIR0Translator 
} // namespace Citron