#pragma once
#include "CommonTranslation.h"
#include <memory>

#include <Syntax/Syntax.h>
#include <IR0/RStructDecl.h>

namespace Citron {

class SkeletonPhaseContext;

std::shared_ptr<RStructDecl> MakeStruct(std::shared_ptr<SStructDecl> sDecl, RTypeDeclOuterPtr rOuter, RAccessor accessor, SkeletonPhaseContext& context);

template<typename ROuter, typename MakeAccessor>
void AddStruct(const std::shared_ptr<ROuter>& rOuter, std::shared_ptr<SStructDecl> sStruct, MakeAccessor makeAccessor, SkeletonPhaseContext& context)
{
    auto accessor = makeAccessor(sStruct->accessModifier);
    auto rStruct = MakeStruct(sStruct, rOuter, accessor, context);
    rOuter->AddType(rStruct);
}


}