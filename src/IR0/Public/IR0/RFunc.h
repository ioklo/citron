#pragma once

#include "RDeclId.h"
#include "RTypeArguments.h"

namespace Citron
{

class RFunc
{
    RDeclIdPtr declId;
    RTypeArgumentsPtr typeArgs;
};

using RFuncPtr = std::shared_ptr<RFunc>;

}