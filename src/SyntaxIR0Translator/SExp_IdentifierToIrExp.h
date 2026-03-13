#pragma once

#include <memory>
#include <expected> 

#include "BodyRes.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class RTypeArguments;
struct ImExp;
struct IrExp;
struct TranslationContexts;
class SExp_Identifier;

std::expected<IrExp*, DiagPtr> TranslateSExp_IdentifierToIrExp(SExp_Identifier* sExp, TranslationContexts& contexts);

} // namespace Citron