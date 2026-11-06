#include "SmStructVarTranslation.h"
#include <vector>

#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RFactory.h"

#include "BuildNonTypeSymbolContext.h"
#include "CommonTranslation.h"
#include "SmPhaseManager.h"
#include "SmTypeTranslation.h"
#include "SmTypeResolveScope.h"
#include "SmTypeTranslationContexts.h"

using namespace std;

namespace Citron {
namespace {

class StructVarTask
    : public IBuildNonTypeSymbolTask
{
    SmDeclContextPtr structDeclContext;
    RStructDecl* rStruct;
    SStructVarDecl* sStructVar;
    RFactoryPtr rFactory;

public:
    StructVarTask(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructVarDecl* sStructVar, InRef<RFactoryPtr> rFactory)
        : structDeclContext{*structDeclContext}, rStruct{rStruct}, sStructVar{sStructVar}, rFactory{*rFactory}
    {
    }

    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateStructVar(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rOuter, SStructVarDecl* syntax, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    shared_ptr<StructVarTask> task{new StructVarTask(structDeclContext, rOuter, syntax, rFactory)};
    phaseManager.AddBuildNonTypeSymbolTask(task);
}

namespace {

expected<void, DiagPtr> StructVarTask::BuildNonTypeSymbol(BuildNonTypeSymbolContext& context)
{
    auto accessor = MakeStructMemberAccessor(sStructVar->accessModifier);
    bool bStatic = false; // TODO: bStatic 지원
    SmTypeTranslationContexts contexts{SmTypeResolveScope_DeclContext{structDeclContext.get()}, rFactory.get()};
    auto e_declType = TranslateSTypeExpToRType(sStructVar->varType, contexts);
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

} // namespace
} // namespace Citron
