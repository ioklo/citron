#pragma once
#include "SyntaxConfig.h"

namespace Citron {

enum class UnaryOpKindSyntax
{
    PostfixInc, PostfixDec,

    Minus, LogicalNot, PrefixInc, PrefixDec,

    Ref, Deref, // &, *, local인지 box인지는 분석을 통해서 알아내게 된다
};

} // namespace Citron
