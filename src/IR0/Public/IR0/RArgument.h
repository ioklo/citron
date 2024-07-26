#pragma once
#include <variant>
#include <memory>

namespace Citron {

using RExpPtr = std::unique_ptr<class RExp>;

class RNormalArgument
{
    RExpPtr exp;
};

class RParamsArgument
{
    RExpPtr exp;
    int elemCount;
};

using RArgument = std::variant<RNormalArgument, RParamsArgument>;

}