#pragma once
#include "SyntaxConfig.h"

#include <variant>
#include <string>
#include "ExpSyntaxes.h"

namespace Citron {

struct TextStringExpElementSyntax
{
    std::u32string text;
};

struct ExpStringExpElementSyntax
{
    ExpSyntax exp;
};

using StringExpElementSyntax = std::variant<TextStringExpElementSyntax, ExpStringExpElementSyntax>;

} // namespace Citron


