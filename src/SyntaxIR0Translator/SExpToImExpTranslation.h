#pragma once

#include <memory>

namespace Citron {

class SExp;
using RTypePtr = std::shared_ptr<class RType>;

namespace SyntaxIR0Translator {

class TranslationContext;
using ImExpPtr = std::shared_ptr<class ImExp>;

ImExpPtr TranslateSExpToImExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron