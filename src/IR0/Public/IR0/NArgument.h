#pragma once

#include <variant>

namespace Citron {

class NExp;

struct NArgument_Normal
{
    NExp* exp;
};

struct NArgument_Params
{
    NExp* exp;
    int elemCount;
};

using NArgument = std::variant<NArgument_Normal, NArgument_Params>;

}