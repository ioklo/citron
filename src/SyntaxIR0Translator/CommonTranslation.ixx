export module Citron.SyntaxIR0Translator:CommonTranslation;

import <vector>;
import <string>;
import Citron.Syntax;

namespace Citron::SyntaxIR0Translator {

export std::vector<std::string> MakeTypeParams(const std::vector<STypeParam>& typeParams);

} // namespace Citron::SyntaxIR0Translator