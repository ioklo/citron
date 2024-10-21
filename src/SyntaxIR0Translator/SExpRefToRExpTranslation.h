#pragma once

#include <memory>

namespace Citron {

class SExp;

using RExpPtr = std::shared_ptr<class RExp>;
using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

class TranslationContext;

RExpPtr TranslateSExpRefToRExp(SExp& exp, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron