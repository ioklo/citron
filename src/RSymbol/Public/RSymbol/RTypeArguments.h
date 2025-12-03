#pragma once

#include "RSymbolConfig.h"

#include <vector>

namespace Citron {

class RType;
class RFactory;

class RTypeArguments
{
    std::vector<RType*> items;

private:
    friend RFactory;
    RTypeArguments(const std::vector<RType*>& items);

public:
    RSYMBOL_API size_t GetCount();
    RSYMBOL_API RType* Get(int i);
    RSYMBOL_API RTypeArguments* Apply(RTypeArguments& typeArgs, RFactory& typeFactory);
};

} // namespace Citron