#pragma once

#include <memory>

namespace Citron {
using NExpPtr = std::shared_ptr<class NExp>;

class RTypeFactory;

using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

class TranslationContext;
class ReExp;

NExpPtr TranslateReExpToNExp(ReExp& reExp, TranslationContext& context);

} // namespace Citron
} // namespace SyntaxIR0Translator