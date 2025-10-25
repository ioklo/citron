#pragma once

#include "ETypeArguments.h"

namespace Citron {

class EDeclId;

class ETypeArguments;

// EFunc는 EDeclId*을 갖고 있다
class EFunc
{
    EDeclId* declId;
    ETypeArguments* typeArgs;
};

} // namespace Citron