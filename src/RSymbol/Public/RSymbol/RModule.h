#pragma once
#include "RSymbolConfig.h"
#include "RNames.h"

namespace Citron {

class RModule
{   
    RName name;
    RNamespaceDecl* rootNamespace;

public:
    RSYMBOL_API RModule(RName&& name, RNamespaceDecl* rootNamespace);
    RName& GetName() { return name; }
};

} // namespace Citron