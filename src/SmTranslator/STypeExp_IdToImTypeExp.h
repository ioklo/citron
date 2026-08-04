#pragma once
#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class STypeExp_Id;
class ImTypeExp;
struct SmTypeTranslationContexts;

std::expected<ImTypeExp, DiagPtr> TranslateSTypeExp_IdToImTypeExp(STypeExp_Id* sTypeExp, SmTypeTranslationContexts& contexts);

} // namespace Citron
