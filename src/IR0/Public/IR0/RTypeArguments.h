#pragma once

#include "IR0Config.h"

#include <vector>
#include <memory>

namespace Citron {

class RType;
class RTypeFactory;

class RTypeArguments
{
    std::vector<RType*> items;

private:
    friend RTypeFactory;
    RTypeArguments(const std::vector<RType*>& items);

public:
    IR0_API size_t GetCount();
    IR0_API RType* Get(int i);
    IR0_API RTypeArguments* Apply(RTypeArguments& typeArgs, RTypeFactory& typeFactory);
};

} // namespace Citron