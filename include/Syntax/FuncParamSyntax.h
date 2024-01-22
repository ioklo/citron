#pragma once
#include "SyntaxConfig.h"
#include <string>
#include "TypeExpSyntaxes.h"

namespace Citron {

struct FuncParamSyntax
{
    bool HasOut;
    bool HasParams;
    TypeExpSyntax Type; 
    std::u32string Name;
};

} // namespace Citron

