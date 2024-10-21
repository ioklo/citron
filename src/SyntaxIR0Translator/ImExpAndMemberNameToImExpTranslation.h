#pragma once

#include <memory>
#include <string>

namespace Citron {

using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;
using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

using ImExpPtr = std::shared_ptr<class ImExp>;
class TranslationContext;

ImExpPtr TranslateImExpAndMemberNameToImExp(ImExp& imExp, const std::string& name, const RTypeArgumentsPtr& typeArgs, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron