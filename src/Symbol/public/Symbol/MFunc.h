#pragma once

#include "MDeclId.h"
#include "MTypeArguments.h"

namespace Citron {

// MFunc는 MDeclIdPtr을 갖고 있다

class MFunc
{
    MDeclIdPtr declId;
    MTypeArgumentsPtr typeArgs;
};

using MFuncPtr = std::shared_ptr<MFunc>;


} // namespace Citron