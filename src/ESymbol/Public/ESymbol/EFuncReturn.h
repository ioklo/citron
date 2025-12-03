#pragma once

#include <variant>

namespace Citron {

class EType;

struct EFuncReturn_ForCtor {}; // for ctor
struct EFuncReturn_Normal
{
    EType* type;
};

using EFuncReturn = std::variant<EFuncReturn_ForCtor, EFuncReturn_Normal>;

}

