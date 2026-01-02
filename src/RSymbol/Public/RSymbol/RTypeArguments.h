#pragma once

#include "RSymbolConfig.h"

#include <vector>

namespace Citron {

class RType;
class RFactory;

class RTypeArguments
{
    std::vector<RType*> items;
    RFactory* factory;

private:
    friend RFactory;
    RTypeArguments(const std::vector<RType*>& items, RFactory* factory);

public:
    RSYMBOL_API size_t GetCount();
    RSYMBOL_API RType* Get(size_t i);
    RSYMBOL_API RTypeArguments* Apply(RTypeArguments& typeArgs);
};

} // namespace Citron