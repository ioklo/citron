#pragma once

#include <memory>

namespace Citron {
using RExpPtr = std::shared_ptr<class RExp>;

class RTypeFactory;

using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

class TranslationContext;
class ReExp;

RExpPtr TranslateReExpToRExp(ReExp& reExp, TranslationContext& context);

} // namespace Citron
} // namespace SyntaxIR0Translator