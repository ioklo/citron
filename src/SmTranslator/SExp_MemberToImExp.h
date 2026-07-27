#pragma once
#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class SExp_Member;
struct ImExp;
struct SmTranslationContexts;

std::expected<ImExp*, DiagPtr> TranslateSExp_MemberToImExp(SExp_Member* sExp, SmTranslationContexts& contexts);

} // namespace Citron
