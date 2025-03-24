#pragma once
#include "CommonTranslation.h"
#include <memory>

#include <Syntax/Syntax.h>
#include <IR0/NStructDecl.h>

namespace Citron {

namespace SyntaxIR0Translator {

class SkeletonPhaseContext;

std::shared_ptr<NStructDecl> InnerMakeStruct(const std::shared_ptr<SStructDecl>& sDecl, const std::shared_ptr<NTypeDeclOuter>& nOuter, RAccessor accessor, SkeletonPhaseContext& context);

template<typename TNOuter, typename TMakeAccessor>
std::shared_ptr<NStructDecl> MakeStruct(const std::shared_ptr<TNOuter>& nOuter, const std::shared_ptr<SStructDecl>& sStruct, TMakeAccessor makeAccessor, SkeletonPhaseContext& context)
{
    auto accessor = makeAccessor(sStruct->accessModifier);
    return InnerMakeStruct(sStruct, nOuter, accessor, context);
}

} // namespace SyntaxIR0Translator

} // namespace Citron