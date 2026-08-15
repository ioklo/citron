#include "EnumElemVarTask.h"
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

void EnumElemVarTask::Register(TakeRef<SmDeclContextPtr> enumElemDeclContext, REnumElemVarDecl* rEnumElemVar, SEnumElemVarDecl* sEnumElemVar, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    shared_ptr<EnumElemVarTask> task{new EnumElemVarTask(move(enumElemDeclContext), rEnumElemVar, sEnumElemVar, move(rFactory))};
    phaseManager.AddBuildNonTypeSymbolTask(task);
}

expected<void, DiagPtr> EnumElemVarTask::BuildNonTypeSymbol(BuildNonTypeSymbolContext& context)
{
    SmTypeTranslationContexts typeTranslationContexts{SmTypeResolveScope_DeclContext{enumElemDeclContext.get()}, rFactory.get()};  

    // enum 기준으로 타입을 만든다
    auto e_rDeclType = TranslateSTypeExpToRType(sEnumElemVar->type, typeTranslationContexts);
    RETURN_ON_ERROR(e_rDeclType);

    rEnumElemVar->InitDeclType(*e_rDeclType);
    rEnumElemVar->GetEnumElem()->AddVar(rEnumElemVar);
    return {};
}

} // namespace Citron