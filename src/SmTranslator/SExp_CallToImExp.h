#pragma once
#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct ImExp;
struct SmTranslationContexts;
class SExp_Call;

std::expected<ImExp*, DiagPtr> TranslateSExp_CallToImExp(SExp_Call* sExp, SmTranslationContexts& contexts);

} // namespace Citron