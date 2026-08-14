#pragma once
#include <memory>
#include "Infra/Ref.h"
#include "TranslationTasks.h"

namespace Citron {

class RStructDecl;
class RStructVarDecl;
class SStructVarDecl;

class SmPhaseManager;

using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class StructVarTask
    : public IBuildNonTypeSymbolTask
{
    SmDeclContextPtr structDeclContext;
    RStructDecl* rStruct;
    SStructVarDecl* sStructVar;
    RFactoryPtr rFactory;

private:
    StructVarTask(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructVarDecl* sStructVar, TakeRef<RFactoryPtr> rFactory)
        : structDeclContext{structDeclContext.Take()}, rStruct{rStruct}, sStructVar{sStructVar}, rFactory{rFactory.Take()}
    {}

public:
    static void Register(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rOuter, SStructVarDecl* syntax, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);
    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
};

} // namespace Citron