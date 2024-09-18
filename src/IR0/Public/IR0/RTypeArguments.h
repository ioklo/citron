#pragma once

#include <vector>
#include <memory>

namespace Citron {

class RType;
using RTypePtr = std::shared_ptr<RType>;

class RTypeArguments
{
    std::vector<RTypePtr> typeArgs;
};

using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

// flyweight
class RTypeArgumentsFactory
{
};


} // namespace Citron