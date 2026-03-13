#pragma once
#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class SExp_Member;
struct ImExp;
struct TranslationContexts;

std::expected<ImExp*, DiagPtr> TranslateSExp_MemberToImExp(SExp_Member* sExp, TranslationContexts& contexts);

} // namespace Citron
