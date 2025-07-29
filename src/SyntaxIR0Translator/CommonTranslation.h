#pragma once

#include <vector>
#include <string>
#include "Syntax/Syntax.h"

namespace Citron::SyntaxIR0Translator {

std::vector<std::string> MakeTypeParams(const std::vector<STypeParam>& typeParams);

} // namespace Citron::SyntaxIR0Translator