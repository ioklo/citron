#pragma once
#include <variant>
#include "RExp.h"

namespace Citron {

class RNormalArgument
{
    RExp exp;
};

class RParamsArgument
{
    RExp exp;
    int elemCount;
};

using RArgument = std::variant<RNormalArgument, RParamsArgument>;

}