#pragma once

#include <memory>
#include <Syntax/Syntax.h>
#include <IR0/REnumDecl.h>

#include "CommonTranslation.h"

namespace Citron {

class SkeletonPhaseContext;

std::shared_ptr<REnumDecl> MakeEnum(RTypeDeclOuterPtr rOuter, SEnumDecl& sDecl, RAccessor accessor, SkeletonPhaseContext& context);

template<typename ROuter, typename MakeAccessor>
void AddEnum(const std::shared_ptr<ROuter>& rOuter, SEnumDecl& sDecl, MakeAccessor makeAccessor, SkeletonPhaseContext& context)
{
    auto accessor = makeAccessor(sDecl.accessModifier);
    auto rDecl = MakeEnum(rOuter, sDecl, accessor, context);
    rOuter->AddType(rDecl);
}

}