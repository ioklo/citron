#include "PhaseManager.h"

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"

#include "MIR/MFuncBody.h"

#include "ResolveTypeHierarchyContext.h"
#include "BuildTypeDependentSymbolContext.h"
// #include "SynthesizeImplicitSymbolContext.h"
#include "TranslateBodyContext.h"

//class BuildTypeDependentSymbolContext;
//class SynthesizeImplicitContext;
//class TranslatingBodyPhaseContext;

//// 2. 함수, 커스텀 타입의 변수들을 트리에 추가한다 (함수들은 인자/리턴 타입을 알아야 만들수 있고, 변수도 타입을 알아야 한다)
//virtual void BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) {}
//
//// 3. struct와 class에 trivial constructor를 추가한다.constructor의 body도 TranslatingBodyPhase에서 만들도록 한다
//virtual void SynthesizeImplicit(SynthesizeImplicitContext& context) {}
//
//// 4. TranslatingBodyPhase : Body를 translation하는 단계.
//virtual void TranslatingBodyPhase(TranslatingBodyPhaseContext& context) {}

using namespace std;

namespace Citron {

PhaseManager::PhaseManager(
    const LoggerPtr& logger, 
    const RFactoryPtr& rFactory, const NFactoryPtr& nFactory, const MFactoryPtr& mFactory,
    const SRTFactoryPtr& srtFactory, const BinOpQueryServicePtr& binOpQueryService)
    : logger{logger}
    , rFactory{rFactory}, nFactory{nFactory}, mFactory{mFactory}, srtFactory{srtFactory}
    , binOpQueryService{binOpQueryService}
{}

PhaseManager::~PhaseManager() = default;

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

expected<vector<MFuncBody>, DiagPtr> PhaseManager::Run()
{
    // 1. ResolveTypeHierarchy

    // 2. BuildTypeDependentSymbol
    BuildTypeDependentSymbolContext fvContext{rFactory, nFactory};
    for (auto&& task : buildTypeDependentSymbolTasks)
        task->BuildTypeDependentSymbol(fvContext);

    // 3. SynthesizeImplicitSymbol, dependency 순서대로 한다

    // 4. TranslateBody
    vector<MFuncBody> funcBodies;
    TranslateBodyContext tbContext{logger, rFactory, mFactory, srtFactory, binOpQueryService};
    for (auto&& task : translatingBodyTasks)
    {
        auto e_result = task->TranslateBody(tbContext);
        RETURN_ON_ERROR(e_result);

        funcBodies.push_back(move(*e_result));
    }

    return funcBodies;
}

} // namespace Citron