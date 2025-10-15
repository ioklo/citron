#pragma once

#include <variant>

namespace Citron {

class MType;

struct MFuncReturn_ForCtor {}; // for ctor
struct MFuncReturn_Normal
{
    MType* type;
};

using MFuncReturn = std::variant<MFuncReturn_ForCtor, MFuncReturn_Normal>;

}

