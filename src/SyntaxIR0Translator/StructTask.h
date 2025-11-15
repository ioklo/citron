#pragma once
#include "TranslationTasks.h"

namespace Citron {

class SStructDecl;
class NStructDecl;

namespace SyntaxIR0Translator {

class PhaseManager;
enum class AccessorContext;

class StructTask
    : public IResolveTypeHierarchyTask
    , public ISynthesizeImplicitSymbolTask
{   
    NStructDecl* nStructDecl;
    SStructDecl* syntax;
    AccessorContext accessorContext;

private:
    StructTask(NStructDecl* nStructDecl, SStructDecl* syntax, AccessorContext accessorContext);

public:
    static void Register(NStructDecl* nStructDecl, SStructDecl* syntax, AccessorContext accessorContext, PhaseManager& phaseManager);
    
    void ResolveTypeHierarchy(ResolveTypeHierarchyContext& context) override;
    void SynthesizeImplicitSymbol(SynthesizeImplicitSymbolContext& context) override;
};

} // namespace SyntaxIR0Translator
} // namespace Citron