#include "StructVarTask.h"
#include <cassert>
#include <vector>

#include "Syntax/Syntax.h"
#include "NSymbol/NStructDecl.h"

#include "BuildTypeDependentSymbolContext.h"
#include "CommonTranslation.h"
#include "PhaseManager.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

void StructVarTask::Register(NStructDecl* nOuter, SStructVarDecl* syntax, const NFactoryPtr& nFactory, PhaseManager& phaseManager)
{
    shared_ptr<StructVarTask> task{new StructVarTask(nOuter, syntax, nFactory)};
    phaseManager.AddBuildTypeDependentSymbolTask(task);
}

void StructVarTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto accessor = MakeAccessor(sStructVar->accessModifier, AccessorContext::InsideStruct);
    bool bStatic = false; // TODO: bStatic 지원
    auto* declType = context.MakeType(sStructVar->varType, nStruct); // decl부분에 자기 자신 대신 outer struct가 들어간다

    vector<NStructVarDecl*> symbols;
    symbols.reserve(sStructVar->varNames.size());

    for (auto& varName : sStructVar->varNames)
    {
        auto* symbol = nFactory->MakeNDecl<NStructVarDecl>(nStruct, accessor, bStatic, varName, declType);
        symbols.push_back(symbol);
        nStruct->AddVar(symbol);
    }
}

} // namespace Citron::SyntaxIR0Translator
