#include "SmPhaseManager.h"

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"

#include "MIR/MFuncBody.h"

#include "SmPhase2Context.h"
#include "SmPhase3Contexts.h"
#include "SmPhase4Context.h"
#include "SmPhase5Contexts.h"
#include "SmPhase6Context.h"
#include "SmPhase7Context.h"

#include "ISmPhase2Task.h"
#include "ISmPhase3Task.h"
#include "ISmPhase4Task.h"
#include "ISmPhase5Task.h"
#include "ISmPhase6Task.h"
#include "ISmPhase7Task.h"

//class SmPhase4Context;
//class SynthesizeImplicitContext;
//class TranslatingBodyPhaseContext;

//// 2. 함수, 커스텀 타입의 변수들을 트리에 추가한다 (함수들은 인자/리턴 타입을 알아야 만들수 있고, 변수도 타입을 알아야 한다)
//virtual expected<void, DiagPtr> RunPhase4(SmPhase4Context& context) {}
//
//// 3. struct와 class에 trivial constructor를 추가한다.constructor의 body도 TranslatingBodyPhase에서 만들도록 한다
//virtual void SynthesizeImplicit(SynthesizeImplicitContext& context) {}
//
//// 4. TranslatingBodyPhase : Body를 translation하는 단계.
//virtual void TranslatingBodyPhase(TranslatingBodyPhaseContext& context) {}

using namespace std;

namespace Citron {

SmPhaseManager::SmPhaseManager(
    TakeRef<LoggerPtr> logger, 
    TakeRef<RFactoryPtr> rFactory, TakeRef<MFactoryPtr> mFactory,
    TakeRef<SmFactoryPtr> smFactory, TakeRef<BinOpQueryServicePtr> binOpQueryService)
    : logger{logger.Take()}
    , rFactory{rFactory.Take()}, mFactory{mFactory.Take()}, smFactory{smFactory.Take()}
    , binOpQueryService{binOpQueryService.Take()}
{}

SmPhaseManager::~SmPhaseManager() = default;

expected<vector<MFuncBody>, DiagPtr> SmPhaseManager::Run()
{
    // 1. Run
    SmPhase2Context phase2Context{rFactory.get()};
    for (auto* task : phase2Tasks)
    {
        auto e_result = task->RunPhase2(phase2Context);
        RETURN_ON_ERROR(e_result);
    }

    SmPhase3Contexts phase3Contexts{};
    for (auto* task : phase3Tasks)
    {
        auto e_result = task->RunPhase3(phase3Contexts);
        RETURN_ON_ERROR(e_result);
    }

    // 2. Run
    SmPhase4Context fvContext{rFactory};
    for (auto* task : phase4Tasks)
    {
        auto e_result = task->RunPhase4(fvContext);
        RETURN_ON_ERROR(e_result);
    }

    SmPhase5Contexts pbContext{this, rFactory.get(), smFactory.get()};
    for (auto* task : phase5Tasks)
    {
        auto e_result = task->RunPhase5(pbContext);
        RETURN_ON_ERROR(e_result);
    }

    // 3. RunPhase6
    SmPhase6Context sisContext{rFactory};
    for (auto* task : phase6Tasks)
        task->RunPhase6(sisContext);

    // 4. RunPhase7
    vector<MFuncBody> funcBodies;
    SmPhase7Context tbContext{logger, rFactory, mFactory, smFactory, binOpQueryService};
    for (auto* task : phase7Tasks)
    {
        auto e_result = task->RunPhase7(tbContext);
        RETURN_ON_ERROR(e_result);

        funcBodies.push_back(move(*e_result));
    }

    return funcBodies;
}

} // namespace Citron