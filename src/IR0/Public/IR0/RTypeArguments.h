#pragma once

#include "IR0Config.h"

#include <vector>

namespace Citron {

class RType;
class IR0Factory;

class RTypeArguments
{
    std::vector<RType*> items;

private:
    friend IR0Factory;
    RTypeArguments(const std::vector<RType*>& items);

public:
    IR0_API size_t GetCount();
    IR0_API RType* Get(int i);
    IR0_API RTypeArguments* Apply(RTypeArguments& typeArgs, IR0Factory& typeFactory);
};

} // namespace Citron