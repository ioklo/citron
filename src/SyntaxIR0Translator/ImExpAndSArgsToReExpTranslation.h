#pragma once

#include <memory>
#include <expected>
#include "ReExp.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class SExp;
class SArguments;
struct ImExp;
struct TranslationContexts;

std::expected<ReExp, DiagPtr> TranslateImExpAndSArgsToReExp(ImExp* imCallable, SArguments* sArgs, TranslationContexts& contexts);

} // namespace Citron