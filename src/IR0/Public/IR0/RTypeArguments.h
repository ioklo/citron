#pragma once

#include "IR0Config.h"

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
    IR0_API size_t GetCount();
    IR0_API RType* Get(int i);
    IR0_API RTypeArguments* Apply(RTypeArguments& typeArgs, RFactory& typeFactory);
};

} // namespace Citron