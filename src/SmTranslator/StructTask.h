#pragma once
#include "TranslationTasks.h"

namespace Citron {

class SStructDecl;
class RStructDecl;

class PhaseManager;
enum class AccessorContext;

class StructTask
    : public IResolveTypeHierarchyTask
    , public ISynthesizeImplicitSymbolTask
{   
    RStructDecl* rStructDecl;
    SStructDecl* syntax;

private:
    StructTask(RStructDecl* rStructDecl, SStructDecl* syntax);

public:
    static void Register(RStructDecl* rStructDecl, SStructDecl* syntax, PhaseManager& phaseManager);
    
    void ResolveTypeHierarchy(ResolveTypeHierarchyContext& context) override;
    void SynthesizeImplicitSymbol(SynthesizeImplicitSymbolContext& context) override;
    

private:
    void SynthesizeMemberwiseCtor(SynthesizeImplicitSymbolContext& context);

    
};

} // namespace Citron