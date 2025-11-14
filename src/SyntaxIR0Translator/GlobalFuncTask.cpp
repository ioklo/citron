#include "GlobalFuncTask.h"

#include "Syntax/Syntax.h"
#include "NSymbol/NGlobalFuncDecl.h"
#include "NSymbol/NNamespaceDecl.h"

#include "CommonTranslation.h"
#include "PhaseManager.h"
#include "BuildTypeDependentSymbolContext.h"
#include "TranslateBodyContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

void GlobalFuncTask::Register(NGlobalFuncDecl* nGFuncDecl, SGlobalFuncDecl* syntax, PhaseManager& phaseManager)
{
    shared_ptr<GlobalFuncTask> task{new GlobalFuncTask(nGFuncDecl, syntax)};
    phaseManager.AddBuildTypeDependentSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

void GlobalFuncTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto accessor = MakeAccessor(syntax->accessModifier, AccessorContext::Global);
    auto typeParams = MakeTypeParams(syntax->typeParams);

    auto* rRetType = context.MakeType(syntax->retType, nGFuncDecl);
    auto [rParameters, bLastParamVariadic] = context.MakeParameters(nGFuncDecl, syntax->parameters);

    nGFuncDecl->InitFuncReturnAndParams(rRetType, move(rParameters), bLastParamVariadic);
}

void GlobalFuncTask::TranslateBody(TranslateBodyContext& context)
{
    context.Translate(nGFuncDecl, syntax->body);
}


} // namespace Citron::SyntaxIR0Translator