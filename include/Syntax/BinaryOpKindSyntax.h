#pragma once

#include "SyntaxConfig.h"

namespace Citron {

enum class BinaryOpKindSyntax
{
    Multiply, Divide, Modulo,

    Add, Subtract,

    LessThan, GreaterThan, LessThanOrEqual, GreaterThanOrEqual,

    Equal, NotEqual,

    Assign,
};

}