#include "StructVarTask.h"
#include <cassert>

#include "Syntax/Syntax.h"
#include "NSymbol/NStructDecl.h"

#include "BuildTypeDependentSymbolContext.h"
#include "CommonTranslation.h"
#include "PhaseManager.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

void StructVarTask::Register(NStructDecl* nOuter, vector<NStructVarDecl*>&& symbols, SStructVarDecl* syntax, PhaseManager& phaseManager)
{
    shared_ptr<StructVarTask> task{new StructVarTask(nOuter, move(symbols), syntax)};
    phaseManager.AddBuildTypeDependentSymbolTask(task);
}

void StructVarTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto accessor = MakeAccessor(syntax->accessModifier, AccessorContext::InsideStruct);
    bool bStatic = false; // TODO: bStatic 지원
    auto declType = context.MakeType(syntax->varType, nOuter); // decl부분에 자기 자신 대신 outer struct가 들어간다

    assert(syntax->varNames.size() == symbols.size());

    for(size_t i = 0, count = symbols.size(); i < count; i++)
        symbols[i]->Init(accessor, bStatic, syntax->varNames[i], declType);
}

} // namespace Citron::SyntaxIR0Translator
