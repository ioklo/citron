#pragma once
#include <variant>
#include <memory>

namespace Citron {

using RExpPtr = std::shared_ptr<class RExp>;

struct RArgument_Normal
{
    RExpPtr exp;
};

struct RArgument_Params
{
    RExpPtr exp;
    int elemCount;
};

using RArgument = std::variant<RArgument_Normal, RArgument_Params>;

}