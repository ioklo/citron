#pragma once
#include <vector>
#include <memory>
#include <concepts>
#include <utility>
#include <expected>
#include "Infra/RefPtr.h"
#include "ISmPhaseTask.h"

namespace Citron {

struct MFuncBody;

using LoggerPtr = std::shared_ptr<class Logger>;
using DiagPtr = std::shared_ptr<struct Diag>;
using MFactoryPtr = std::shared_ptr<class MFactory>;
using RFactoryPtr = std::shared_ptr<class RFactory>;

using SmFactoryPtr = std::shared_ptr<class SmFactory>;
using BinOpQueryServicePtr = std::shared_ptr<class BinOpQueryService>;

// Phase1 : BuildTypeSymbolPhase (body-space에서 만들어지는 lambda 제외)
// Phase2 : BuildTypeHierarchyPhase (상속 구조 확정과, type alias resolve를 같이 진행합니다)
// Phase3 : PostBuildTypeHierarchyPhase, (BuildTrait)
// Phase4 : BuildNonTypeSymbolPhase (func, var)
// Phase5 : PostBuildNonTypeSymbolPhase (impl ...)
// Phase6 : SynthesizeImplicitPhase (memberwise-ctor)
// Phase7 : TranslateBodyPhase 

class ISmPhase2Task;
class ISmPhase3Task;
class ISmPhase4Task;
class ISmPhase5Task;
class ISmPhase6Task;
class ISmPhase7Task;

class SmPhaseManager
{   
    LoggerPtr logger;
    RFactoryPtr rFactory;
    MFactoryPtr mFactory;

    SmFactoryPtr smFactory;
    BinOpQueryServicePtr binOpQueryService;

    std::vector<std::unique_ptr<ISmPhaseTask>> phaseTasks;

    std::vector<ISmPhase2Task*> phase2Tasks;
    std::vector<ISmPhase3Task*> phase3Tasks;
    std::vector<ISmPhase4Task*> phase4Tasks;
    std::vector<ISmPhase5Task*> phase5Tasks;
    std::vector<ISmPhase6Task*> phase6Tasks;
    std::vector<ISmPhase7Task*> phase7Tasks;

    bool running{false};

public:
    SmPhaseManager(
        InRefPtr<Logger> logger, 
        InRefPtr<RFactory> rFactory, InRefPtr<MFactory> mFactory,
        InRefPtr<SmFactory> smFactory, InRefPtr<BinOpQueryService> binOpQueryService);
    ~SmPhaseManager();

    template<typename TSmPhaseTask, typename... TArgs>
    void AddTask(TArgs&&... args);
    std::expected<std::vector<MFuncBody>, DiagPtr> Run();
};

} // namespace Citron

// implementation

namespace Citron {

template<typename TSmPhaseTask, typename... TArgs>
void SmPhaseManager::AddTask(TArgs&&... args)
{
    auto task = std::make_unique<TSmPhaseTask>(std::forward<TArgs>(args)...);
    auto* pTask = task.get();

    phaseTasks.push_back(move(task));

    assert(!running);

    if constexpr (std::derived_from<TSmPhaseTask, ISmPhase2Task>) phase2Tasks.push_back(pTask);
    if constexpr (std::derived_from<TSmPhaseTask, ISmPhase3Task>) phase3Tasks.push_back(pTask);
    if constexpr (std::derived_from<TSmPhaseTask, ISmPhase4Task>) phase4Tasks.push_back(pTask);
    if constexpr (std::derived_from<TSmPhaseTask, ISmPhase5Task>) phase5Tasks.push_back(pTask);
    if constexpr (std::derived_from<TSmPhaseTask, ISmPhase6Task>) phase6Tasks.push_back(pTask);
    if constexpr (std::derived_from<TSmPhaseTask, ISmPhase7Task>) phase7Tasks.push_back(pTask);

    static_assert(std::derived_from<TSmPhaseTask, ISmPhase2Task>
        || std::derived_from<TSmPhaseTask, ISmPhase3Task>
        || std::derived_from<TSmPhaseTask, ISmPhase4Task>
        || std::derived_from<TSmPhaseTask, ISmPhase5Task>
        || std::derived_from<TSmPhaseTask, ISmPhase6Task>
        || std::derived_from<TSmPhaseTask, ISmPhase7Task>);
}

} // namespace Citron