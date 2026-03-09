#pragma once

#include <memory>
#include <string>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

class RTypeArguments;
struct ImExp;
struct TranslationContexts;

std::expected<ImExp*, DiagPtr> TranslateImExpAndMemberNameToImExp(ImExp* imExp, const std::string& name, RTypeArguments* typeArgs, TranslationContexts& contexts);

} // namespace Citron