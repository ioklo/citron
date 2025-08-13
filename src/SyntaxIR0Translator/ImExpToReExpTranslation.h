#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron::SyntaxIR0Translator {

class ReExp;
class ImExp;
class TranslationContext;

std::expected<ReExp*, DiagPtr> TranslateImExpToReExp(ImExp& imExp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator