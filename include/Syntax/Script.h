#pragma once
#include "SyntaxConfig.h"
#include <vector>

#include "ScriptElements.h"

namespace Citron::Syntax {

// °¡Àå ¿Ü°û
class Script
{
    std::vector<ScriptElement> Elements;
};

} // namespace Citron::Syntax
