#pragma once
#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct ImExp;
struct TranslationContexts;
class SExp_Call;

std::expected<ImExp*, DiagPtr> TranslateSExp_CallToImExp(SExp_Call* sExp, TranslationContexts& contexts);

} // namespace Citron