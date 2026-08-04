#pragma once
#include <expected>
#include <memory>
#include <span>
#include "RSymbol/RAppliedDecl.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

class STypeExp;
class RType;
class RTypeArguments;
class RTraitDecl;
class SmTypeRes;

struct SmTypeTranslationContexts;

std::expected<RType*, DiagPtr> TranslateSTypeExpToRType(STypeExp* sTypeExp, SmTypeTranslationContexts& contexts);
std::expected<RAppliedDecl<RTraitDecl>, DiagPtr> TranslateSTypeExpToRTrait(STypeExp* sTypeExp, SmTypeTranslationContexts& contexts);
std::expected<RType*, DiagPtr> MakeType(SmTypeRes& typeRes, std::span<STypeExp*> sMemberTypeArgs, SmTypeTranslationContexts& contexts);
std::expected<RTypeArguments*, DiagPtr> MakeRTypeArguments(std::span<STypeExp*> typeArgs, SmTypeTranslationContexts& contexts);
std::expected<RTypeArguments*, DiagPtr> MakeRTypeArguments(RTypeArguments* outerTypeArgs, std::span<STypeExp*> sMemberTypeArgs, SmTypeTranslationContexts& contexts);


} // namespace Citron
