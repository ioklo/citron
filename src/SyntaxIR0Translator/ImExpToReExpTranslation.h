#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron::SyntaxIR0Translator {

class ReExp;
using ReExpPtr = std::shared_ptr<ReExp>;

class ImExp;
class TranslationContext;

std::expected<ReExpPtr, DiagPtr> TranslateImExpToReExp(ImExp& imExp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator