#include "PhaseManager.h"

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"

#include "MIR/MFuncBody.h"

#include "BuildTypeHierarchyContext.h"
#include "BuildNonTypeSymbolContext.h"
#include "BuildImplicitSymbolContext.h"
#include "TranslateBodyContext.h"

//class BuildNonTypeSymbolContext;
//class SynthesizeImplicitContext;
//class TranslatingBodyPhaseContext;

//// 2. 함수, 커스텀 타입의 변수들을 트리에 추가한다 (함수들은 인자/리턴 타입을 알아야 만들수 있고, 변수도 타입을 알아야 한다)
//virtual expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) {}
//
//// 3. struct와 class에 trivial constructor를 추가한다.constructor의 body도 TranslatingBodyPhase에서 만들도록 한다
//virtual void SynthesizeImplicit(SynthesizeImplicitContext& context) {}
//
//// 4. TranslatingBodyPhase : Body를 translation하는 단계.
//virtual void TranslatingBodyPhase(TranslatingBodyPhaseContext& context) {}

using namespace std;

namespace Citron {

PhaseManager::PhaseManager(
    TakeRef<LoggerPtr> logger, 
    TakeRef<RFactoryPtr> rFactory, TakeRef<MFactoryPtr> mFactory,
    TakeRef<SmFactoryPtr> smFactory, TakeRef<BinOpQueryServicePtr> binOpQueryService)
    : logger{logger.Take()}
    , rFactory{rFactory.Take()}, mFactory{mFactory.Take()}, smFactory{smFactory.Take()}
    , binOpQueryService{binOpQueryService.Take()}
{}

PhaseManager::~PhaseManager() = default;

void PhaseManager::AddBuildTypeHierarchyTask(std::shared_ptr<IBuildTypeHierarchyTask>&& task)
{
    buildTypeHierarchyTasks.push_back(move(task));
}

void PhaseManager::AddBuildNonTypeSymbolTask(std::shared_ptr<IBuildNonTypeSymbolTask>&& task)
{
    buildNonTypeSymbolTasks.push_back(move(task));
}

void PhaseManager::AddPostBuildNonTypeSymbolTask(std::shared_ptr<IPostBuildNonTypeSymbolTask>&& task)
{
    postBuildNonTypeSymbolTasks.push_back(move(task));
}

void PhaseManager::AddBuildImplicitSymbolTask(std::shared_ptr<IBuildImplicitSymbolTask>&& task)
{
    buildImplicitSymbolTasks.push_back(move(task));
}

void PhaseManager::AddTranslateBodyTask(std::shared_ptr<ITranslateBodyTask>&& task)
{
    translatingBodyTasks.push_back(move(task));
}

expected<vector<MFuncBody>, DiagPtr> PhaseManager::Run()
{
    // 1. BuildTypeHierarchy
    BuildTypeHierarchyContext rthContext{rFactory.get()};
    for (auto& task : buildTypeHierarchyTasks)
    {
        auto e_result = task->BuildTypeHierarchy(rthContext);
        RETURN_ON_ERROR(e_result);
    }

    // 2. BuildNonTypeSymbol
    BuildNonTypeSymbolContext fvContext{rFactory};
    for (auto& task : buildNonTypeSymbolTasks)
    {
        auto e_result = task->BuildNonTypeSymbol(fvContext);
        RETURN_ON_ERROR(e_result);
    }

    // 3. BuildImplicitSymbol
    BuildImplicitSymbolContext sisContext{rFactory};
    for (auto& task : buildImplicitSymbolTasks)
        task->BuildImplicitSymbol(sisContext);

    // 4. TranslateBody
    vector<MFuncBody> funcBodies;
    TranslateBodyContext tbContext{logger, rFactory, mFactory, smFactory, binOpQueryService};
    for (auto& task : translatingBodyTasks)
    {
        auto e_result = task->TranslateBody(tbContext);
        RETURN_ON_ERROR(e_result);

        funcBodies.push_back(move(*e_result));
    }

    return funcBodies;
}

} // namespace Citron