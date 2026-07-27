#pragma once

#include <memory>
#include <expected>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class SExp;
class RType;
struct ImExp;
struct SmTranslationContexts;

std::expected<ImExp*, DiagPtr> TranslateSExpToImExp(SExp* sExp, RType* hintType, SmTranslationContexts& contexts);

} // namespace Citron