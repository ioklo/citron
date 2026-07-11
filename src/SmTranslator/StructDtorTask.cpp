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

expected<void, DiagPtr> StructDtorTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto accessor = MakeStructMemberAccessor(sStructDtor->accessModifier);

    // 굳이 type이 없어도 만들수는 있지만 그냥 여기서 만들자
    rStructDtor = rFactory->MakeDecl<RStructDtorDecl>(rStruct, accessor);


    return {};
}

std::expected<MFuncBody, DiagPtr> StructDtorTask::TranslateBody(TranslateBodyContext& context)
{
    return context.Translate(rStructDtor, sStructDtor->body);
}

void StructDtorTask::Register(RStructDecl* rStruct, SStructDtorDecl* sStructDtor, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{
    auto task = MakePtr<StructDtorTask>(rStruct, sStructDtor, move(rFactory));

    phaseManager.AddBuildTypeDependentSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

} // namespace Citron