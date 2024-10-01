#pragma once
#include "CommonTranslation.h"
#include <memory>

#include <Syntax/Syntax.h>
#include <IR0/RStructDecl.h>

namespace Citron {

namespace SyntaxIR0Translator {

class SkeletonPhaseContext;

std::shared_ptr<RStructDecl> InnerMakeStruct(std::shared_ptr<SStructDecl> sDecl, RTypeDeclOuterPtr rOuter, RAccessor accessor, SkeletonPhaseContext& context);

template<typename TROuter, typename TMakeAccessor>
std::shared_ptr<RStructDecl> MakeStruct(const std::shared_ptr<TROuter>& rOuter, std::shared_ptr<SStructDecl> sStruct, TMakeAccessor makeAccessor, SkeletonPhaseContext& context)
{
    auto accessor = makeAccessor(sStruct->accessModifier);
    return InnerMakeStruct(sStruct, rOuter, accessor, context);
}

} // namespace SyntaxIR0Translator

} // namespace Citron