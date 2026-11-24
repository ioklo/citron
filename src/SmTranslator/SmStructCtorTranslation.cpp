#include "SmStructCtorTranslation.h"

#include "Infra/Expected.h"
#include "Infra/Ptr.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructCtorDecl.h"
#include "MIR/MFuncBody.h"

#include "CommonTranslation.h"
#include "SmPhase4Context.h"
#include "SmPhase7Context.h"
#include "SmPhaseManager.h"
#include "SmDeclContext_Decl.h"
#include "SmTypeTranslation.h"
#include "SmTypeResolveScope.h"
#include "ISmPhaseTask.h"
#include "ISmPhase4Task.h"
#include "ISmPhase7Task.h"

using namespace std;

namespace Citron {
namespace {

class StructCtorTask
    : public ISmPhaseTask
    , public ISmPhase4Task
    , public ISmPhase7Task
{
    SmDeclContextPtr structDeclContext;
    RStructDecl* rStruct;
    SStructCtorDecl* sStructCtor;

    RStructCtorDecl* rStructCtor;
    RFactoryPtr rFactory;

public:
    StructCtorTask(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructCtorDecl* sStructCtor, InRef<RFactoryPtr> rFactory)
        : structDeclContext{*structDeclContext}, rStruct{rStruct}, sStructCtor{sStructCtor}, rStructCtor{nullptr}, rFactory{*rFactory}
    {
    }

public:
    std::expected<void, DiagPtr> RunPhase4(SmPhase4Context& context) override;
    std::expected<MFuncBody, DiagPtr> RunPhase7(SmPhase7Context& context) override;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateStructCtor(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructCtorDecl* sStructCtor, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    phaseManager.AddTask<StructCtorTask>(structDeclContext, rStruct, sStructCtor, rFactory);
}


namespace {

expected<void, DiagPtr> StructCtorTask::RunPhase4(SmPhase4Context& context)
{
    auto accessor = MakeStructMemberAccessor(sStructCtor->accessModifier);
    rStructCtor = rFactory->MakeDecl<RStructCtorDecl>(rStruct, accessor, RStructCtorKind::Normal);

    vector<RTypeParam*> typeParams{};
    SmTypeResolveScope_DeclHeader scope{structDeclContext.get(), typeParams};
    auto e_parameters = context.MakeFuncParameters(sStructCtor->parameters, scope);
    RETURN_ON_ERROR_REFDECL(e_parameters, [parameters, bLastParamVariadic]);
    rStructCtor->Init(RDeclKey::Ctor(parameters), vector<RTypeParam*>{}, move(parameters), bLastParamVariadic);
    rStruct->AddCtor(rStructCtor);

    return {};
}

expected<MFuncBody, DiagPtr> StructCtorTask::RunPhase7(SmPhase7Context& context)
{
    SmDeclContextPtr declContext = MakePtr<SmDeclContext_Decl<RStructCtorDecl>>(structDeclContext, rStructCtor, rStructCtor->MakeOpenTypeArgs(*rFactory));
    return context.Translate(move(declContext), rStructCtor, /*bSeqFunc*/false, sStructCtor->body);
}

} // namespace 
} // namespace Citron
