#pragma once

#include <memory>
#include <expected>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

struct IrExp;
struct TranslationContexts;
class SExp_Member;

std::expected<IrExp*, DiagPtr> TranslateSExp_MemberToIrExp(SExp_Member* sExp, TranslationContexts& contexts);

} // namespace Citron