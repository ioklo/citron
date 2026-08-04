#pragma once
#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class STypeExp;
class ImTypeExp;
struct SmTypeTranslationContexts;

std::expected<ImTypeExp, DiagPtr> TranslateSTypeExpToImTypeExp(STypeExp* sTypeExp, SmTypeTranslationContexts& contexts);


} // namespace Citron
