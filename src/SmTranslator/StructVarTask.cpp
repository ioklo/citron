#include "StructVarTask.h"
#include <cassert>
#include <vector>

#include "Syntax/Syntax.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RFactory.h"

#include "BuildNonTypeSymbolContext.h"
#include "CommonTranslation.h"
#include "PhaseManager.h"

using namespace std;

namespace Citron {

void StructVarTask::Register(RStructDecl* rOuter, SStructVarDecl* syntax, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{
    shared_ptr<StructVarTask> task{new StructVarTask(rOuter, syntax, move(rFactory))};
    phaseManager.AddBuildNonTypeSymbolTask(task);
}

expected<void, DiagPtr> StructVarTask::BuildNonTypeSymbol(BuildNonTypeSymbolContext& context)
{
    auto accessor = MakeStructMemberAccessor(sStructVar->accessModifier);
    bool bStatic = false; // TODO: bStatic 지원
    auto* declType = context.MakeType(sStructVar->varType, rStruct); // decl부분에 자기 자신 대신 outer struct가 들어간다

    vector<RStructVarDecl*> symbols;
    symbols.reserve(sStructVar->varNames.size());

    for (auto& varName : sStructVar->varNames)
    {
        auto* symbol = rFactory->MakeDecl<RStructVarDecl>(rStruct, accessor, bStatic, declType, RName::Normal(varName), rStruct->GetVarCount());
        symbols.push_back(symbol);
        rStruct->AddVar(symbol);
    }

    return {};
}

} // namespace Citron
