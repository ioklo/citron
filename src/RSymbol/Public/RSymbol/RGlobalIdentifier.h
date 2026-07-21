#pragma once
#include "RSymbolConfig.h"
#include <string>

namespace Citron {

class RDecl;

struct RGlobalIdentifier
{
    std::string text;
};

RSYMBOL_API RGlobalIdentifier GetRGlobalIdentifier(RDecl* decl);

} // namespace Citron 
