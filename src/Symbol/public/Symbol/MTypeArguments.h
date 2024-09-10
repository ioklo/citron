#pragma once

#include <vector>
#include <memory>

namespace Citron {

class MType;
using MTypePtr = std::shared_ptr<MType>;

class MTypeArguments
{
    std::vector<MTypePtr> typeArgs;
};

using MTypeArgumentsPtr = std::shared_ptr<MTypeArguments>;

// flyweight
class MTypeArgumentsFactory
{

};


} // namespace Citron