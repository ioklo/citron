#pragma once

#include <memory>
#include <string>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

namespace SyntaxIR0Translator {

class ImExp;
using ImExpPtr = std::shared_ptr<ImExp>;

class TranslationContext;

std::expected<ImExpPtr, DiagPtr> TranslateImExpAndMemberNameToImExp(ImExp& imExp, const std::string& name, const RTypeArgumentsPtr& typeArgs, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron