#pragma once
import <vector>;
import <string>;
#include <IR0/NDecl.h>

namespace Citron {

class STypeParam;

namespace SyntaxIR0Translator {

std::vector<std::string> MakeTypeParams(const std::vector<STypeParam>& typeParams);

} // namespace SyntaxIR0Translator

} // namespace Citron