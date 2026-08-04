#pragma once
#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class STypeExp_Member;
class ImTypeExp;
struct SmTypeTranslationContexts;

std::expected<ImTypeExp, DiagPtr> TranslateSTypeExp_MemberToImTypeExp(STypeExp_Member* sTypeExp, SmTypeTranslationContexts& contexts);

} // namespace Citron