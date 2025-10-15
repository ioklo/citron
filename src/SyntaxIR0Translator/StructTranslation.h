#pragma once

#include "CommonTranslation.h"
#include <memory>

#include "Syntax/Syntax.h"
#include "IR0/RAccessor.h"

namespace Citron {

class NStructDecl;
class NTypeDeclOuter;

namespace SyntaxIR0Translator {

class SkeletonPhaseContext;

NStructDecl* InnerMakeStruct(SStructDecl* sDecl, NTypeDeclOuter* nOuter, RAccessor accessor, SkeletonPhaseContext& context);

template<typename TNOuter, typename TMakeAccessor>
NStructDecl* MakeStruct(TNOuter* nOuter, SStructDecl* sStruct, TMakeAccessor makeAccessor, SkeletonPhaseContext& context)
{
    auto accessor = makeAccessor(sStruct->accessModifier);
    return InnerMakeStruct(sStruct, nOuter, accessor, context);
}

} // namespace SyntaxIR0Translator
} // namespace Citron
