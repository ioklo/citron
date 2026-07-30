#include "StructVarTask.h"
#include <cassert>
#include <vector>

#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RFactory.h"

#include "BuildNonTypeSymbolContext.h"
#include "CommonTranslation.h"
#include "PhaseManager.h"
#include "SmTypeTranslation.h"

using namespace std;

namespace Citron {

void StructVarTask::Register(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rOuter, SStructVarDecl* syntax, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{
    shared_ptr<StructVarTask> task{new StructVarTask(move(structDeclContext), rOuter, syntax, move(rFactory))};
    phaseManager.AddBuildNonTypeSymbolTask(task);
}

expected<void, DiagPtr> StructVarTask::BuildNonTypeSymbol(BuildNonTypeSymbolContext& context)
{
    auto accessor = MakeStructMemberAccessor(sStructVar->accessModifier);
    bool bStatic = false; // TODO: bStatic 지원
    auto e_declType = TranslateSTypeExpToRType(sStructVar->varType, SmTypeResolveScope_DeclContext{structDeclContext.get()}, rFactory.get());
    RETURN_ON_ERROR(e_declType);

    vector<RStructVarDecl*> symbols;
    symbols.reserve(sStructVar->varNames.size());

    for (auto& varName : sStructVar->varNames)
    {
        auto* symbol = rFactory->MakeDecl<RStructVarDecl>(RDeclKey::Normal(RName::Normal(varName)), rStruct, accessor, bStatic, *e_declType, RName::Normal(varName), rStruct->GetVarCount());
        symbols.push_back(symbol);
        rStruct->AddVar(symbol);
    }

    return {};
}

} // namespace Citron
