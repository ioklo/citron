#pragma once
#include "SyntaxConfig.h"
#include "Exps.h"

namespace Citron::Syntax {

struct Argument
{
    bool bOut;
    bool bParams;
    Exp exp;
};

} // namespace Citron::Syntax
 