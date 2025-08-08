#pragma once

#include <vector>

namespace Citron {

class MType;

class MTypeArguments
{
    std::vector<MType*> typeArgs;
};

// flyweight
class MTypeArgumentsFactory
{
};


} // namespace Citron