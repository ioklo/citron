#pragma once
#include <vector>
#include <string>
#include <IR0/RDecl.h>

namespace Citron {

class STypeParam;

namespace SyntaxIR0Translator {

std::vector<std::string> MakeTypeParams(const std::vector<STypeParam>& typeParams);

} // namespace SyntaxIR0Translator

} // namespace Citron