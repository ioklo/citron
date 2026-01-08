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
    size_t GetCount() { return items.size(); }
    RType* Get(size_t i) { return items[i]; }
    RSYMBOL_API RTypeArguments* Apply(RTypeArguments& typeArgs);
};

} // namespace Citron