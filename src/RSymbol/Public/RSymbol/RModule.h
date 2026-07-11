#pragma once
#include "RSymbolConfig.h"
#include "RNames.h"

namespace Citron {

class RModule
{   RName name;

public:
    RSYMBOL_API RModule(RName&& name);
    RName& GetName() { return name; }
};

} // namespace Citron