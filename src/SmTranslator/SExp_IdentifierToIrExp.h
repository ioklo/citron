#pragma once

#include <memory>
#include <expected> 

#include "SmBodyRes.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class RTypeArguments;
struct ImExp;
struct IrExp;
struct SmTranslationContexts;
class SExp_Identifier;

std::expected<IrExp*, DiagPtr> TranslateSExp_IdentifierToIrExp(SExp_Identifier* sExp, SmTranslationContexts& contexts);

} // namespace Citron