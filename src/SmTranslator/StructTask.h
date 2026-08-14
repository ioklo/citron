#pragma once
#include <memory>
#include "Infra/Ref.h"
#include "TranslationTasks.h"

namespace Citron {

class SStructDecl;
class RStructDecl;

class SmPhaseManager;
enum class AccessorContext;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class StructTask
    : public IBuildTypeHierarchyTask
    , public IBuildImplicitSymbolTask
{   
    SmDeclContextPtr structDeclContext;
    RStructDecl* rStructDecl;
    SStructDecl* syntax;

private:
    StructTask(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStructDecl, SStructDecl* syntax);

public:
    static void Register(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStructDecl, SStructDecl* syntax, SmPhaseManager& phaseManager);
    
    std::expected<void, DiagPtr> BuildTypeHierarchy(BuildTypeHierarchyContext& context) override;
    void BuildImplicitSymbol(BuildImplicitSymbolContext& context) override;
    

private:
    void SynthesizeMemberwiseCtor(BuildImplicitSymbolContext& context);

    
};

} // namespace Citron