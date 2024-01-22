#pragma once
#include "SyntaxConfig.h"
#include "ExpSyntaxes.h"

namespace Citron {

struct ArgumentSyntax
{
    bool bOut;
    bool bParams;
    ExpSyntax exp;
};

} // namespace Citron
 