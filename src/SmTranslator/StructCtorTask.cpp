#include "StructCtorTask.h"

#include "Infra/Expected.h"
#include "Infra/Ptr.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructCtorDecl.h"
#include "MIR/MFuncBody.h"

#include "CommonTranslation.h"
#include "BuildNonTypeSymbolContext.h"
#include "TranslateBodyContext.h"
#include "PhaseManager.h"
#include "SmDeclContext_Decl.h"

using namespace std;

namespace Citron {

void StructCtorTask::Register(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructCtorDecl* sStructCtor, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{
    shared_ptr<StructCtorTask> task{new StructCtorTask{move(structDeclContext), rStruct, sStructCtor, std::move(rFactory)}};

    phaseManager.AddBuildNonTypeSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

expected<void, DiagPtr> StructCtorTask::BuildNonTypeSymbol(BuildNonTypeSymbolContext& context)
{
    auto accessor = MakeStructMemberAccessor(sStructCtor->accessModifier);

    SmFuncHeaderResolveScope scope{rStruct, {}};
    auto e_parameters = context.MakeParameters(sStructCtor->parameters, scope);
    RETURN_ON_ERROR_REFDECL(e_parameters, [parameters, bLastParamVariadic]);
    
    rStructCtor = rFactory->MakeDecl<RStructCtorDecl>(RDeclKey::Ctor(parameters), rStruct, accessor, RStructCtorKind::Normal,
        vector<RTypeParam*>{}, move(parameters), bLastParamVariadic);
    rStruct->AddCtor(rStructCtor);

    return {};
}

expected<MFuncBody, DiagPtr> StructCtorTask::TranslateBody(TranslateBodyContext& context)
{
    SmDeclContextPtr declContext = MakePtr<SmDeclContext_Decl<RStructCtorDecl>>(structDeclContext, rStructCtor, rStructCtor->MakeOpenTypeArgs(*rFactory));
    return context.Translate(move(declContext), rStructCtor, /*bSeqFunc*/false, sStructCtor->body);
}


} // namespace Citron
