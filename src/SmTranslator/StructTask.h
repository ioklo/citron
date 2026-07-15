#pragma once
#include "TranslationTasks.h"

namespace Citron {

class SStructDecl;
class RStructDecl;

class PhaseManager;
enum class AccessorContext;

class StructTask
    : public IBuildTypeHierarchyTask
    , public IBuildImplicitSymbolTask
{   
    RStructDecl* rStructDecl;
    SStructDecl* syntax;

private:
    StructTask(RStructDecl* rStructDecl, SStructDecl* syntax);

public:
    static void Register(RStructDecl* rStructDecl, SStructDecl* syntax, PhaseManager& phaseManager);
    
    void BuildTypeHierarchy(BuildTypeHierarchyContext& context) override;
    void BuildImplicitSymbol(BuildImplicitSymbolContext& context) override;
    

private:
    void SynthesizeMemberwiseCtor(BuildImplicitSymbolContext& context);

    
};

} // namespace Citron