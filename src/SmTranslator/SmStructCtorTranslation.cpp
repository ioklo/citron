#include "SmStructCtorTranslation.h"

#include "Infra/Expected.h"
#include "Infra/Ptr.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructCtorDecl.h"
#include "MIR/MFuncBody.h"

#include "CommonTranslation.h"
#include "BuildNonTypeSymbolContext.h"
#include "TranslateBodyContext.h"
#include "SmPhaseManager.h"
#include "SmDeclContext_Decl.h"
#include "SmTypeTranslation.h"
#include "SmTypeResolveScope.h"

using namespace std;

namespace Citron {
namespace {

class StructCtorTask
    : public IBuildNonTypeSymbolTask
    , public ITranslateBodyTask
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
    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateStructCtor(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructCtorDecl* sStructCtor, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    shared_ptr<StructCtorTask> task{new StructCtorTask{structDeclContext, rStruct, sStructCtor, rFactory}};

    phaseManager.AddBuildNonTypeSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}


namespace {

expected<void, DiagPtr> StructCtorTask::BuildNonTypeSymbol(BuildNonTypeSymbolContext& context)
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

expected<MFuncBody, DiagPtr> StructCtorTask::TranslateBody(TranslateBodyContext& context)
{
    SmDeclContextPtr declContext = MakePtr<SmDeclContext_Decl<RStructCtorDecl>>(structDeclContext, rStructCtor, rStructCtor->MakeOpenTypeArgs(*rFactory));
    return context.Translate(move(declContext), rStructCtor, /*bSeqFunc*/false, sStructCtor->body);
}

} // namespace 
} // namespace Citron
