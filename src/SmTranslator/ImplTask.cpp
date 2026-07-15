#include "ImplTask.h"
#include <memory>
#include "Infra/Exceptions.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RDecl.h"
#include "MIR/MFuncBody.h"
#include "PhaseManager.h"

using namespace std;

namespace Citron {

void ImplTask::Register(SImplDecl* implDecl, RDecl* outer, PhaseManager& phaseManager)
{
    shared_ptr<ImplTask> task{new ImplTask{implDecl, outer}};
    phaseManager.AddPostBuildNonTypeSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

std::expected<void, DiagPtr> ImplTask::PostBuildNonTypeSymbol(PostBuildNonTypeSymbolContext& context)
{
    // SImplDecl을 NImplTrait로 만든다
    // impl S : TraitName

    // impl의 name은 다른 부분과 다르게 현재 scope에 있는 struct/class/enum 이름이다
    auto* rTypeDecl = rOuter->GetTypeMember(RName::Normal(sImplDecl->name));
    

    return {};
}

std::expected<MFuncBody, DiagPtr> ImplTask::TranslateBody(TranslateBodyContext& context)
{
    // TODO: [66]
    throw NotImplementedException{};
}

} // namespace Citron