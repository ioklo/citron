#include "StructDtorTask.h"
#include "Infra/Ptr.h"

#include "Syntax/Syntaxes.g.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructDtorDecl.h"
#include "RSymbol/RFactory.h"

#include "MIR/MFuncBody.h"

#include "CommonTranslation.h"

#include "PhaseManager.h"
#include "TranslateBodyContext.h"
#include "SmDeclContext_Decl.h"

using namespace std;

namespace Citron {

StructDtorTask::StructDtorTask(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructDtorDecl* sStructDtor, TakeRef<RFactoryPtr> rFactory)
    : structDeclContext{structDeclContext.Take()}, rStruct{rStruct}, sStructDtor {sStructDtor}, rFactory{rFactory.Take()}, rStructDtor{nullptr}
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

void StructDtorTask::Register(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructDtorDecl* sStructDtor, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{
    auto task = MakePtr<StructDtorTask>(move(structDeclContext), rStruct, sStructDtor, move(rFactory));

    phaseManager.AddBuildNonTypeSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

} // namespace Citron