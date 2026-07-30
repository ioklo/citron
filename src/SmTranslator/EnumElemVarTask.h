#pragma once
#include "Infra/Ref.h"
#include "TranslationTasks.h"


namespace Citron {

class SEnumDecl;
class SEnumElemDecl;
class SEnumElemVarDecl;

class REnumDecl;
class REnumElemDecl;
class REnumElemVarDecl;

class PhaseManager;
enum class AccessorContext;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class EnumElemVarTask
    : public IBuildNonTypeSymbolTask
{
    SmDeclContextPtr enumElemDeclContext;
    REnumElemVarDecl* rEnumElemVar;
    SEnumElemVarDecl* sEnumElemVar;
    RFactoryPtr rFactory;

    EnumElemVarTask(TakeRef<SmDeclContextPtr> enumElemDeclContext, REnumElemVarDecl* rEnumElemVar, SEnumElemVarDecl* sEnumElemVar, TakeRef<RFactoryPtr> rFactory)
        : enumElemDeclContext{enumElemDeclContext.Take()}, rEnumElemVar{rEnumElemVar}, sEnumElemVar{sEnumElemVar}, rFactory{rFactory.Take()}
    {
    }

public:
    static void Register(TakeRef<SmDeclContextPtr> enumElemDeclContext, REnumElemVarDecl* rEnumElemVar, SEnumElemVarDecl* sEnumElemVar, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager);
    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
};

} // namespace Citron

