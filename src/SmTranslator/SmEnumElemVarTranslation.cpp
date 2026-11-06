#include "SmEnumElemVarTranslation.h"
#include "Infra/Expected.h"

#include "RSymbol/REnumDecl.h"
#include "RSymbol/REnumElemDecl.h"
#include "RSymbol/REnumElemVarDecl.h"
#include "SmPhaseManager.h"
#include "CommonTranslation.h"
#include "BuildNonTypeSymbolContext.h"
#include "SmTypeTranslation.h"
#include "SmTypeTranslationContexts.h"
#include "SmTypeResolveScope.h"

using namespace std;

namespace Citron {
namespace {

class SmEnumElemVarTranslator
    : public IBuildNonTypeSymbolTask
{
    SmDeclContextPtr enumElemDeclContext;
    REnumElemVarDecl* rEnumElemVar;
    SEnumElemVarDecl* sEnumElemVar;
    RFactoryPtr rFactory;

public:
    SmEnumElemVarTranslator(InRef<SmDeclContextPtr> enumElemDeclContext, REnumElemVarDecl* rEnumElemVar, SEnumElemVarDecl* sEnumElemVar, InRef<RFactoryPtr> rFactory)
        : enumElemDeclContext{*enumElemDeclContext}, rEnumElemVar{rEnumElemVar}, sEnumElemVar{sEnumElemVar}, rFactory{*rFactory}
    {
    }

    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateEnumElemVar(InRef<SmDeclContextPtr> enumElemDeclContext, SEnumElemVarDecl* sEnumElemVar, REnumElemDecl* rEnumElem, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    RName elemVarName{RName::Normal(sEnumElemVar->name)};
    auto* rEnumElemVar = (*rFactory)->MakeDecl<REnumElemVarDecl>(RDeclKey::Normal(elemVarName), rEnumElem, move(elemVarName));

    shared_ptr<SmEnumElemVarTranslator> task{new SmEnumElemVarTranslator(enumElemDeclContext, rEnumElemVar, sEnumElemVar, rFactory)};
    phaseManager.AddBuildNonTypeSymbolTask(task);
}

namespace {

expected<void, DiagPtr> SmEnumElemVarTranslator::BuildNonTypeSymbol(BuildNonTypeSymbolContext& context)
{
    SmTypeTranslationContexts typeTranslationContexts{SmTypeResolveScope_DeclContext{enumElemDeclContext.get()}, rFactory.get()};

    // enum 기준으로 타입을 만든다
    auto e_rDeclType = TranslateSTypeExpToRType(sEnumElemVar->type, typeTranslationContexts);
    RETURN_ON_ERROR(e_rDeclType);

    rEnumElemVar->InitDeclType(*e_rDeclType);
    rEnumElemVar->GetEnumElem()->AddVar(rEnumElemVar);
    return {};
}

} // namespace
} // namespace Citron