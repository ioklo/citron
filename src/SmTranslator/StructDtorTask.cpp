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

using namespace std;

namespace Citron {

StructDtorTask::StructDtorTask(RStructDecl* rStruct, SStructDtorDecl* sStructDtor, TakeRef<RFactoryPtr> rFactory)
    : rStruct{rStruct}, sStructDtor {sStructDtor}, rFactory{rFactory.Take()}, rStructDtor{nullptr}
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
    return context.Translate(rStructDtor, /*bSeqFunc*/false, sStructDtor->body);
}

void StructDtorTask::Register(RStructDecl* rStruct, SStructDtorDecl* sStructDtor, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{
    auto task = MakePtr<StructDtorTask>(rStruct, sStructDtor, move(rFactory));

    phaseManager.AddBuildNonTypeSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

} // namespace Citron