#include "PhaseManager.h"

#include "Infra/Exceptions.h"

#include "BuildTypeSymbolContext.h"
#include "BuildTypeDependentSymbolContext.h"

//class BuildTypeSymbolContext;
//class BuildTypeDependentSymbolContext;
//class SynthesizeImplicitContext;
//class TranslatingBodyPhaseContext;

//// 1. 네임스페이스와 커스텀 타입들을 트리에 추가한다
//virtual void BuildTypeSymbol(BuildTypeSymbolContext& context) {}
//
//// 2. 함수, 커스텀 타입의 변수들을 트리에 추가한다 (함수들은 인자/리턴 타입을 알아야 만들수 있고, 변수도 타입을 알아야 한다)
//virtual void BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) {}
//
//// 3. struct와 class에 trivial constructor를 추가한다.constructor의 body도 TranslatingBodyPhase에서 만들도록 한다
//virtual void SynthesizeImplicit(SynthesizeImplicitContext& context) {}
//
//// 4. TranslatingBodyPhase : Body를 translation하는 단계.
//virtual void TranslatingBodyPhase(TranslatingBodyPhaseContext& context) {}

namespace Citron {
namespace SyntaxIR0Translator {

PhaseManager::PhaseManager() = default;
PhaseManager::~PhaseManager() = default;

void PhaseManager::AddBuildTypeSymbolTask(std::shared_ptr<IBuildTypeSymbolTask>&& task)
{
    buildTypeSymbolTasks.push_back(move(task));
}

void PhaseManager::AddResolveTypeHierarchyTask(std::shared_ptr<IResolveTypeHierarchyTask>&& task)
{
    resolveTypeHierarchyTasks.push_back(move(task));
}

void PhaseManager::AddBuildTypeDependentSymbolTask(std::shared_ptr<IBuildTypeDependentSymbolTask>&& task)
{
    buildTypeDependentSymbolTasks.push_back(move(task));
}

void PhaseManager::AddSynthesizeImplicitSymbolTask(std::shared_ptr<ISynthesizeImplicitSymbolTask>&& task)
{
    synthesizeImplicitSymbolTask.push_back(move(task));
}

void PhaseManager::AddTranslateBodyTask(std::shared_ptr<ITranslateBodyTask>&& task)
{
    translatingBodyTasks.push_back(move(task));
}

void PhaseManager::Run()
{
    // 1. 
    BuildTypeSymbolContext stContext{nFactory};
    for(auto&& task : buildTypeSymbolTasks)
        task->BuildTypeSymbol(stContext);

    // 2. 
    
    BuildTypeDependentSymbolContext fvContext{nFactory};
    for (auto&& task : buildTypeDependentSymbolTasks)
        task->BuildTypeDependentSymbol(fvContext);



    throw NotImplementedException{};

}

} // namespace SyntaxIR0Translator 
} // namespace Citron