#include "StructCtorTask.h"

#include "NSymbol/NStructDecl.h"
#include "NSymbol/NStructCtorDecl.h"
#include "MIR/MFuncBody.h"

#include "CommonTranslation.h"
#include "BuildTypeDependentSymbolContext.h"
#include "TranslateBodyContext.h"
#include "PhaseManager.h"

using namespace std;

namespace Citron::SyntaxIR0Translator
{

void StructCtorTask::Register(NStructDecl* nStruct, SStructCtorDecl* sStructCtor, const NFactoryPtr& nFactory, PhaseManager& phaseManager)
{
    shared_ptr<StructCtorTask> task{new StructCtorTask{nStruct, sStructCtor, nFactory}};

    phaseManager.AddBuildTypeDependentSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

void StructCtorTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto accessor = MakeAccessor(sStructCtor->accessModifier, AccessorContext::InsideStruct);
    // TODO: 타이프 쳐서 만들어진 ctor는 'trivial' 표시를 하기 전까지는 trivial로 인식하지 않는다. 지금은 false로 표기
    // 그리고 컴파일러가 trivial 조건을 체크해서 에러를 낼 수도 있다 (하위 타입의 trivial constructor가 이 constructor를 참조하지 않는다)
    bool bTrivial = false;

    nStructCtor = nFactory->MakeNDecl<NStructCtorDecl>(nStruct, accessor, bTrivial);
    nStruct->AddCtor(nStructCtor);

    // symbol tree에 매달린 nStructCtor가 필요
    auto [parameters, bLastParamVariadic] = context.MakeParameters(nStructCtor, sStructCtor->parameters);
    nStructCtor->InitFuncParameters(move(parameters), bLastParamVariadic);
}

expected<MFuncBody, DiagPtr> StructCtorTask::TranslateBody(TranslateBodyContext& context)
{
    return context.Translate(nStructCtor, sStructCtor->body);
}


} // namespace Citron::SyntaxIR0Translator
