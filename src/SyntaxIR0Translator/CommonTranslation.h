#pragma once

#include <vector>
#include <string>
#include <optional>

#include "Syntax/Syntax.h"

namespace Citron {

enum class RAccessor;
class NTypeParamDecl;
class NFactory;
class NDecl;

enum class AccessorContext
{
    Global,
    InsideStruct,
    InsideClass,
};

RAccessor MakeAccessor(std::optional<SAccessModifier> modifier, AccessorContext context);

std::vector<NTypeParamDecl*> MakeTypeParams(NDecl* outer, const std::vector<STypeParam>& sTypeParams, NFactory& nFactory);

} // namespace Citron