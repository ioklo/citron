#include "EnumElemVarTask.h"
#include "NSymbol/NEnumDecl.h"
#include "NSymbol/NEnumElemDecl.h"
#include "NSymbol/NEnumElemVarDecl.h"
#include "PhaseManager.h"
#include "CommonTranslation.h"
#include "BuildTypeDependentSymbolContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

void EnumElemVarTask::Register(NEnumElemVarDecl* nEnumElemVar, SEnumElemVarDecl* sEnumElemVar, PhaseManager& phaseManager)
{
    shared_ptr<EnumElemVarTask> task{new EnumElemVarTask(nEnumElemVar, sEnumElemVar)};
    phaseManager.AddBuildTypeDependentSymbolTask(task);
}

void EnumElemVarTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{   
    // enum 기준으로 타입을 만든다
    auto* rDeclType = context.MakeType(sEnumElemVar->type, nEnumElemVar->enumElem->_enum);
    nEnumElemVar->Init(rDeclType);
    nEnumElemVar->enumElem->AddVar(nEnumElemVar);
}

} // namespace Citron::SyntaxIR0Translator