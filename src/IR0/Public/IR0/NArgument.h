#pragma once
#include <variant>
#include <memory>

namespace Citron {

using NExpPtr = std::shared_ptr<class NExp>;

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