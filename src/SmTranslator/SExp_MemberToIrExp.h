#pragma once

#include <memory>
#include <expected>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

struct IrExp;
struct SmTranslationContexts;
class SExp_Member;

std::expected<IrExp*, DiagPtr> TranslateSExp_MemberToIrExp(SExp_Member* sExp, SmTranslationContexts& contexts);

} // namespace Citron