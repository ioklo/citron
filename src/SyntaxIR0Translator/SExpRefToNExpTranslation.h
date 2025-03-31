#pragma once

import <memory>;

namespace Citron {

class SExp;

using NExpPtr = std::shared_ptr<class NExp>;
using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

class TranslationContext;

NExpPtr TranslateSExpRefToNExp(SExp& exp, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron