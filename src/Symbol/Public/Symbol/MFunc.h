#pragma once

#include "MTypeArguments.h"

namespace Citron {

class MDeclId;

class MTypeArguments;

// MFunc는 MDeclId*을 갖고 있다
class MFunc
{
    MDeclId* declId;
    MTypeArguments* typeArgs;
};

} // namespace Citron