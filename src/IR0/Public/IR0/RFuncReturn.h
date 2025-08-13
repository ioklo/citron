#pragma once

#include <variant>

namespace Citron {

class RType;

struct RFuncReturn_ForCtor {};
struct RFuncReturn_Set
{
    RType* type;
};
struct RFuncReturn_NotSet {}; // need inference
using RFuncReturn = std::variant<RFuncReturn_ForCtor, RFuncReturn_Set, RFuncReturn_NotSet>;

}

