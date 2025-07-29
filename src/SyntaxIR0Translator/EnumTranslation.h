#pragma once

#include <memory>
#include "Syntax/Syntax.h"
#include "IR0/RAccessor.h"

#include "CommonTranslation.h"

namespace Citron {

class NTypeDeclOuter;
using NTypeDeclOuterWPtr = std::weak_ptr<NTypeDeclOuter>;

class NEnumDecl;

namespace SyntaxIR0Translator {

class SkeletonPhaseContext;

std::shared_ptr<NEnumDecl> InnerMakeEnum(NTypeDeclOuterWPtr nOuter, SEnumDecl& sDecl, RAccessor accessor, SkeletonPhaseContext& context);

template<typename TNOuter, typename TMakeAccessor>
std::shared_ptr<NEnumDecl> MakeEnum(const std::shared_ptr<TNOuter>& rOuter, SEnumDecl& sDecl, TMakeAccessor makeAccessor, SkeletonPhaseContext& context)
{
    auto accessor = makeAccessor(sDecl.accessModifier);
    return InnerMakeEnum(rOuter, sDecl, accessor, context);
}

} // namespace SyntaxIR0Translator
} // namespace Citron