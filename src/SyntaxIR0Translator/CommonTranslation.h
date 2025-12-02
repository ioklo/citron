#pragma once

#include <vector>
#include <string>
#include <optional>

#include "Syntax/Syntax.h"

namespace Citron {

enum class RAccessor;

enum class AccessorContext
{
    Global,
    InsideStruct,
    InsideClass,
};

RAccessor MakeAccessor(std::optional<SAccessModifier> modifier, AccessorContext context);

std::vector<std::string> MakeTypeParams(const std::vector<STypeParam>& typeParams);

} // namespace Citron