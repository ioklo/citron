#pragma once

#include <variant>

namespace Citron {

class MExp;
class MLoc;

struct MArgument_Exp
{
    MExp* exp;
};

struct MArgument_Ref
{
    MLoc* loc;
};

struct MArgument_Params
{
    MExp* exp;
    int elemCount;
};

using MArgument = std::variant<MArgument_Exp, MArgument_Ref, MArgument_Params>;

}