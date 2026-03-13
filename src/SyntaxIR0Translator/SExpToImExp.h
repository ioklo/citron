#pragma once

#include <memory>
#include <expected>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class SExp;
class RType;
struct ImExp;
struct TranslationContexts;

std::expected<ImExp*, DiagPtr> TranslateSExpToImExp(SExp* sExp, RType* hintType, TranslationContexts& contexts);

} // namespace Citron