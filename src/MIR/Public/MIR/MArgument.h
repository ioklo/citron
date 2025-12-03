#pragma once

#include <variant>

namespace Citron {

class MExp;

struct MArgument_Normal
{
    MExp* exp;
};

struct MArgument_Params
{
    MExp* exp;
    int elemCount;
};

using MArgument = std::variant<MArgument_Normal, MArgument_Params>;

}