#pragma once

#include "SyntaxConfig.h"

namespace Citron::Syntax {

enum class BinaryOpKind
{
    Multiply, Divide, Modulo,

    Add, Subtract,

    LessThan, GreaterThan, LessThanOrEqual, GreaterThanOrEqual,

    Equal, NotEqual,

    Assign,
};

}