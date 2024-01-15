#pragma once
#include "SyntaxConfig.h"
#include <string>
#include "TypeExps.h"

namespace Citron::Syntax {

struct FuncParam
{
    bool HasOut;
    bool HasParams;
    TypeExp Type; 
    std::string Name;
};

} // namespace Citron::Syntax

