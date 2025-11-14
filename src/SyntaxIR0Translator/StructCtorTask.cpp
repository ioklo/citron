#include "StructCtorTask.h"

#include "NSymbol/NStructCtorDecl.h"

#include "CommonTranslation.h"
#include "BuildTypeDependentSymbolContext.h"
#include "TranslateBodyContext.h"
#include "PhaseManager.h"

using namespace std;

namespace Citron::SyntaxIR0Translator
{

void StructCtorTask::Register(NStructCtorDecl* nFuncDecl, SStructCtorDecl* syntax, PhaseManager& phaseManager)
{
    shared_ptr<StructCtorTask> task{new StructCtorTask{nFuncDecl, syntax}};

    phaseManager.AddBuildTypeDependentSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

void StructCtorTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto accessor = MakeAccessor(syntax->accessModifier, AccessorContext::InsideStruct);

    // TODO: 타이프 쳐서 만들어진 ctor는 'trivial' 표시를 하기 전까지는 trivial로 인식하지 않는다. 지금은 false로 표기
    // 그리고 컴파일러가 trivial 조건을 체크해서 에러를 낼 수도 있다 (하위 타입의 trivial constructor가 이 constructor를 참조하지 않는다)
    symbol->Init(accessor, /*bTrivial*/false);

    // ctor는 Type Parameter가 없으므로 파라미터를 만들 때, 상위(struct) declSymbol을 넘긴다
    auto [parameters, bLastParamVariadic] = context.MakeParameters(symbol, syntax->parameters);
    symbol->InitFuncParameters(move(parameters), bLastParamVariadic);
}

void StructCtorTask::TranslateBody(TranslateBodyContext& context)
{
    context.Translate(symbol, syntax->body);
}


} // namespace Citron::SyntaxIR0Translator
