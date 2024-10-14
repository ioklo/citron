#pragma once

#include <memory>
#include <variant>

namespace Citron {

using RTypePtr = std::shared_ptr<class RType>;

struct RFuncReturn_ForConstructor {}; // for constructor
struct RFuncReturn_Set
{
    RTypePtr type;
};
struct RFuncReturn_NotSet {}; // need inference
using RFuncReturn = std::variant<RFuncReturn_ForConstructor, RFuncReturn_Set, RFuncReturn_NotSet>;

}

