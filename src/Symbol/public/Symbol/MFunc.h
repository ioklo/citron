#pragma once

#include <memory>

#include "MTypeArguments.h"

namespace Citron {

class MDeclId;
using MDeclIdPtr = std::shared_ptr<MDeclId>;

class MTypeArguments;
using MTypeArgumentsPtr = std::shared_ptr<MTypeArguments>;

// MFunc는 MDeclIdPtr을 갖고 있다
class MFunc
{
    MDeclIdPtr declId;
    MTypeArgumentsPtr typeArgs;
};

using MFuncPtr = std::shared_ptr<MFunc>;


} // namespace Citron