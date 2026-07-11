#include "EnumElemVarTask.h"
#include "RSymbol/REnumDecl.h"
#include "RSymbol/REnumElemDecl.h"
#include "RSymbol/REnumElemVarDecl.h"
#include "PhaseManager.h"
#include "CommonTranslation.h"
#include "BuildTypeDependentSymbolContext.h"

using namespace std;

namespace Citron {

void EnumElemVarTask::Register(REnumElemVarDecl* rEnumElemVar, SEnumElemVarDecl* sEnumElemVar, PhaseManager& phaseManager)
{
    shared_ptr<EnumElemVarTask> task{new EnumElemVarTask(rEnumElemVar, sEnumElemVar)};
    phaseManager.AddBuildTypeDependentSymbolTask(task);
}

expected<void, DiagPtr> EnumElemVarTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{   
    // enum 기준으로 타입을 만든다
    auto* rDeclType = context.MakeType(sEnumElemVar->type, rEnumElemVar->GetEnumElem()->GetEnum());
    rEnumElemVar->InitDeclType(rDeclType);
    rEnumElemVar->GetEnumElem()->AddVar(rEnumElemVar);

    return {};
}

} // namespace Citron