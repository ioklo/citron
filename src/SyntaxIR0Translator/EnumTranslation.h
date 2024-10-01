#pragma once

#include <memory>
#include <Syntax/Syntax.h>
#include <IR0/REnumDecl.h>

#include "CommonTranslation.h"

namespace Citron {

namespace SyntaxIR0Translator {

class SkeletonPhaseContext;

std::shared_ptr<REnumDecl> InnerMakeEnum(RTypeDeclOuterPtr rOuter, SEnumDecl& sDecl, RAccessor accessor, SkeletonPhaseContext& context);

template<typename TROuter, typename TMakeAccessor>
std::shared_ptr<REnumDecl> MakeEnum(const std::shared_ptr<TROuter>& rOuter, SEnumDecl& sDecl, TMakeAccessor makeAccessor, SkeletonPhaseContext& context)
{
    auto accessor = makeAccessor(sDecl.accessModifier);
    return InnerMakeEnum(rOuter, sDecl, accessor, context);
}

} // namespace SyntaxIR0Translator

} // namespace Citron