#include "SmStructVarTranslation.h"
#include <vector>

#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RFactory.h"

#include "SmPhase4Context.h"
#include "CommonTranslation.h"
#include "SmPhaseManager.h"
#include "SmTypeTranslation.h"
#include "SmTypeResolveScope.h"
#include "SmTypeTranslationContexts.h"
#include "ISmPhaseTask.h"
#include "ISmPhase4Task.h"

using namespace std;

namespace Citron {

using SmDeclContextPtr = shared_ptr<class SmDeclContext>;

namespace {

class StructVarTask
    : public ISmPhaseTask
    , public ISmPhase4Task
{
    SmDeclContextPtr structDeclContext;
    RStructDecl* rStruct;
    SStructVarDecl* sStructVar;
    RFactoryPtr rFactory;

public:
    StructVarTask(InRefPtr<SmDeclContext> structDeclContext, RStructDecl* rStruct, SStructVarDecl* sStructVar, InRefPtr<RFactory> rFactory)
        : structDeclContext{structDeclContext.Share()}, rStruct{rStruct}, sStructVar{sStructVar}, rFactory{rFactory.Share()}
    {
    }

    std::expected<void, DiagPtr> RunPhase4(SmPhase4Context& context) override;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateStructVar(InRefPtr<SmDeclContext> structDeclContext, RStructDecl* rOuter, SStructVarDecl* syntax, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager)
{
    phaseManager.AddTask<StructVarTask>(structDeclContext, rOuter, syntax, rFactory);
}

namespace {

expected<void, DiagPtr> StructVarTask::RunPhase4(SmPhase4Context& context)
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
