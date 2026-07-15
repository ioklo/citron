#pragma once
#include <memory>
#include "Infra/Ref.h"
#include "TranslationTasks.h"

namespace Citron {

class RStructDecl;
class RStructVarDecl;
class SStructVarDecl;

class PhaseManager;

using RFactoryPtr = std::shared_ptr<class RFactory>;

class StructVarTask
    : public IBuildNonTypeSymbolTask
{
    RStructDecl* rStruct;
    SStructVarDecl* sStructVar;
    RFactoryPtr rFactory;

private:
    StructVarTask(RStructDecl* rStruct, SStructVarDecl* sStructVar, TakeRef<RFactoryPtr> rFactory)
        : rStruct{rStruct}, sStructVar{sStructVar}, rFactory{rFactory.Take()}
    {}

public:
    static void Register(RStructDecl* rOuter, SStructVarDecl* syntax, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager);
    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
};

} // namespace Citron