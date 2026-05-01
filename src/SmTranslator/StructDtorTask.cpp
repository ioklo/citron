#include "StructDtorTask.h"
#include "Infra/Ptr.h"

#include "Syntax/Syntaxes.g.h"
#include "NSymbol/NStructDecl.h"
#include "NSymbol/NStructDtorDecl.h"
#include "NSymbol/NFactory.h"

#include "MIR/MFuncBody.h"

#include "CommonTranslation.h"

#include "PhaseManager.h"
#include "TranslateBodyContext.h"

using namespace std;

namespace Citron {

StructDtorTask::StructDtorTask(NStructDecl* nStruct, SStructDtorDecl* sStructDtor, const NFactoryPtr& nFactory)
    : nStruct{nStruct}, sStructDtor {sStructDtor}, nFactory{nFactory}, nStructDtor{nullptr}
{
}

expected<void, DiagPtr> StructDtorTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto accessor = MakeAccessor(sStructDtor->accessModifier, AccessorContext::InsideStruct);

    // 굳이 type이 없어도 만들수는 있지만 그냥 여기서 만들자
    nStructDtor = nFactory->MakeNDecl<NStructDtorDecl>(accessor, nStruct);

    return {};
}

std::expected<MFuncBody, DiagPtr> StructDtorTask::TranslateBody(TranslateBodyContext& context)
{
    return context.Translate(nStructDtor, sStructDtor->body);
}

void StructDtorTask::Register(NStructDecl* nStruct, SStructDtorDecl* sStructDtor, const NFactoryPtr& nFactory, PhaseManager& phaseManager)
{
    auto task = MakePtr<StructDtorTask>(nStruct, sStructDtor, nFactory);

    phaseManager.AddBuildTypeDependentSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

} // namespace Citron