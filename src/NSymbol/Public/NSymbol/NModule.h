#pragma once
#include "NSymbolConfig.h"

#include <string>

#include "RSymbol/RModule.h"

namespace Citron {

class NNamespaceDecl;

class NModule : public RModule
{
public:
    std::string name;

public:
    NSYMBOL_API NModule(std::string&& name);
};

}