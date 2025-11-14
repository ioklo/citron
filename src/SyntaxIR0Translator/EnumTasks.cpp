#include "EnumTasks.h"
#include "NSymbol/NEnumDecl.h"
#include "NSymbol/NEnumElemDecl.h"
#include "NSymbol/NEnumElemVarDecl.h"
#include "PhaseManager.h"
#include "CommonTranslation.h"
#include "BuildTypeSymbolContext.h"
#include "BuildTypeDependentSymbolContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

void EnumTask::Register(NEnumDecl* nEnum, SEnumDecl* sEnum, AccessorContext accessorContext, PhaseManager& phaseManager)
{
    shared_ptr<EnumTask> task{new EnumTask(nEnum, sEnum, accessorContext)};
    phaseManager.AddBuildTypeSymbolTask(task);
}

void EnumTask::BuildTypeSymbol(BuildTypeSymbolContext& context)
{
    auto accessor = MakeAccessor(sEnum->accessModifier, accessorContext);
    auto typeParams = MakeTypeParams(sEnum->typeParams);

    nEnum->Init(accessor, RName_Normal(sEnum->name), move(typeParams));
}

void EnumElemTask::Register(NEnumElemDecl* nEnumElem, SEnumElemDecl* sEnumElem, PhaseManager& phaseManager)
{
    shared_ptr<EnumElemTask> task{new EnumElemTask(nEnumElem, sEnumElem)};
    phaseManager.AddBuildTypeSymbolTask(task);
}

void EnumElemTask::BuildTypeSymbol(BuildTypeSymbolContext& context)
{
    nEnumElem->Init(sEnumElem->name);
}

void EnumElemVarTask::Register(NEnumElemVarDecl* nEnumElemVar, SEnumElemVarDecl* sEnumElemVar, PhaseManager& phaseManager)
{
    shared_ptr<EnumElemVarTask> task{new EnumElemVarTask(nEnumElemVar, sEnumElemVar)};
    phaseManager.AddBuildTypeDependentSymbolTask(task);
}

void EnumElemVarTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{   
    auto* rDeclType = context.MakeType(sEnumElemVar->type, nEnumElemVar->GetNOuter());
    nEnumElemVar->Init(sEnumElemVar->name, rDeclType);
}

} // namespace Citron::SyntaxIR0Translator