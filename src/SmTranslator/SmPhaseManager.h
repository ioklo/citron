#pragma once
#include <vector>
#include <memory>
#include <expected>
#include "Infra/Ref.h"

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

    std::vector<std::shared_ptr<ISmPhase2Task>> phase2Tasks;
    std::vector<std::shared_ptr<ISmPhase3Task>> phase3Tasks;
    std::vector<std::shared_ptr<ISmPhase4Task>> phase4Tasks;
    std::vector<std::shared_ptr<ISmPhase5Task>> phase5Tasks;
    std::vector<std::shared_ptr<ISmPhase6Task>> phase6Tasks;
    std::vector<std::shared_ptr<ISmPhase7Task>> phase7Tasks;

public:
    SmPhaseManager(
        TakeRef<LoggerPtr> logger, 
        TakeRef<RFactoryPtr> rFactory, TakeRef<MFactoryPtr> mFactory,
        TakeRef<SmFactoryPtr> smFactory, TakeRef<BinOpQueryServicePtr> binOpQueryService);
    ~SmPhaseManager(); 

    void AddPhase2Task(std::shared_ptr<ISmPhase2Task>&& task);
    void AddPhase3Task(std::shared_ptr<ISmPhase3Task>&& task);
    void AddPhase4Task(std::shared_ptr<ISmPhase4Task>&& task);
    void AddPhase5Task(std::shared_ptr<ISmPhase5Task>&& task);
    void AddPhase6Task(std::shared_ptr<ISmPhase6Task>&& task);
    void AddPhase7Task(std::shared_ptr<ISmPhase7Task>&& task);

    std::expected<std::vector<MFuncBody>, DiagPtr> Run();
};


} // namespace Citron