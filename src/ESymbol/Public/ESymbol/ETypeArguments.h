#pragma once

#include <vector>

namespace Citron {

class EType;

class ETypeArguments
{
    std::vector<EType*> typeArgs;
};

// flyweight
class ETypeArgumentsFactory
{
};


} // namespace Citron