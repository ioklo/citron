#include "StructFuncTask.h"
#include "NSymbol/NStructFuncDecl.h"
#include "BuildTypeDependentSymbolContext.h"
#include "TranslateBodyContext.h"
#include "CommonTranslation.h"
#include "PhaseManager.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

void StructFuncTask::Register(NStructFuncDecl* nFuncDecl, SStructFuncDecl* syntax, PhaseManager& phaseManager)
{
    shared_ptr<StructFuncTask> task{new StructFuncTask(nFuncDecl, syntax)};
    phaseManager.AddBuildTypeDependentSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

void StructFuncTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto accessor = MakeAccessor(syntax->accessModifier, AccessorContext::InsideStruct);
    auto typeParams = MakeTypeParams(syntax->typeParams);

    nFuncDecl->Init(accessor, syntax->name, move(typeParams), syntax->bStatic);

    auto* rRetType = context.MakeType(syntax->retType, nFuncDecl);
    auto [rParameters, bLastParamVariadic] = context.MakeParameters(nFuncDecl, syntax->parameters);

    nFuncDecl->InitFuncReturnAndParams(rRetType, move(rParameters), bLastParamVariadic);
}

void StructFuncTask::TranslateBody(TranslateBodyContext& context)
{
    context.Translate(nFuncDecl, syntax->body);
}


}