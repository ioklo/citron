#pragma once
#include "SyntaxConfig.h"

#include <variant>
#include <string>
#include "Exps.h"

namespace Citron::Syntax {

struct TextStringExpElement
{
    std::string text;
};

// recursive, { (StringExp)exp
struct ExpStringExpElement
{
    Exp exp;
};

using StringExpElement = std::variant<TextStringExpElement, ExpStringExpElement>;

} // namespace Citron::Syntax


