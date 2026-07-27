#pragma once
#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct ImExp;
struct SmTranslationContexts;
class SExp_Identifier;

std::expected<ImExp*, DiagPtr> TranslateSExp_IdentifierToImExp(SExp_Identifier* sExp, SmTranslationContexts& contexts);

} // namespace Citron
