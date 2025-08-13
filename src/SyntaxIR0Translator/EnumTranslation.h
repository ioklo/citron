#pragma once

#include <memory>
#include "Syntax/Syntax.h"
#include "IR0/RAccessor.h"

#include "CommonTranslation.h"

namespace Citron {

class NTypeDeclOuter;
class NEnumDecl;

namespace SyntaxIR0Translator {

class SkeletonPhaseContext;

NEnumDecl* InnerMakeEnum(NTypeDeclOuter* nOuter, SEnumDecl& sDecl, RAccessor accessor, SkeletonPhaseContext& context);

template<typename TNOuter, typename TMakeAccessor>
NEnumDecl* MakeEnum(TNOuter* rOuter, SEnumDecl& sDecl, TMakeAccessor makeAccessor, SkeletonPhaseContext& context)
{
    auto accessor = makeAccessor(sDecl.accessModifier);
    return InnerMakeEnum(rOuter, sDecl, accessor, context);
}

} // namespace SyntaxIR0Translator
} // namespace Citron