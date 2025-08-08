#pragma once

#include <variant>
#include <memory>

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