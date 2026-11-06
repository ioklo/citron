#include "SmStructDtorTranslation.h"
#include "Infra/Ptr.h"

#include "Syntax/Syntaxes.g.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructDtorDecl.h"
#include "RSymbol/RFactory.h"

#include "MIR/MFuncBody.h"

#include "CommonTranslation.h"

#include "SmPhaseManager.h"
#include "TranslateBodyContext.h"
#include "SmDeclContext_Decl.h"

using namespace std;

namespace Citron {
namespace {

class StructDtorTask
    : public IBuildNonTypeSymbolTask
    , public ITranslateBodyTask
{
    SmDeclContextPtr structDeclContext;
    RStructDecl* rStruct;
    SStructDtorDecl* sStructDtor;
    RFactoryPtr rFactory;

    RStructDtorDecl* rStructDtor;

public:
    StructDtorTask(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructDtorDecl* sStructDtor, InRef<RFactoryPtr> rFactory);
    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateStructDtor(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructDtorDecl* sStructDtor, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    auto task = MakePtr<StructDtorTask>(structDeclContext, rStruct, sStructDtor, rFactory);
    phaseManager.AddBuildNonTypeSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

namespace {

StructDtorTask::StructDtorTask(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructDtorDecl* sStructDtor, InRef<RFactoryPtr> rFactory)
    : structDeclContext{*structDeclContext}, rStruct{rStruct}, sStructDtor{sStructDtor}, rFactory{*rFactory}, rStructDtor{nullptr}
{
}

expected<void, DiagPtr> StructDtorTask::BuildNonTypeSymbol(BuildNonTypeSymbolContext& context)
{
    auto accessor = MakeStructMemberAccessor(sStructDtor->accessModifier);
    rStructDtor = rFactory->MakeDecl<RStructDtorDecl>(RDeclKey::Dtor(), rStruct, accessor);
    return {};
}

std::expected<MFuncBody, DiagPtr> StructDtorTask::TranslateBody(TranslateBodyContext& context)
{
    SmDeclContextPtr declContext = MakePtr<SmDeclContext_Decl<RStructDtorDecl>>(structDeclContext, rStructDtor, rStructDtor->MakeOpenTypeArgs(*rFactory));
    return context.Translate(declContext, rStructDtor, /*bSeqFunc*/false, sStructDtor->body);
}

} // namespace
} // namespace Citron