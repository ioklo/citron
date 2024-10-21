#pragma once

#include <memory>

namespace Citron {

using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

using ReExpPtr = std::shared_ptr<class ReExp>;
class ImExp;
class TranslationContext;

ReExpPtr TranslateImExpToReExp(ImExp& imExp, TranslationContext& context);

} // namespace SyntaxIR0Translator

} // namespace Citron