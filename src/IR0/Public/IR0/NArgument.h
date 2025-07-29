#pragma once

#include <variant>
#include <memory>

namespace Citron {

class NExp;
using NExpPtr = std::shared_ptr<NExp>;

struct NArgument_Normal
{
    NExpPtr exp;
};

struct NArgument_Params
{
    NExpPtr exp;
    int elemCount;
};

using NArgument = std::variant<NArgument_Normal, NArgument_Params>;

}