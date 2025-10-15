#pragma once

#include <memory>
#include <string>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

class RTypeArguments;

namespace SyntaxIR0Translator {

class ImExp;

class TranslationContext;

std::expected<ImExp*, DiagPtr> TranslateImExpAndMemberNameToImExp(ImExp* imExp, const std::string& name, RTypeArguments* typeArgs, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron