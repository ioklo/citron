#pragma once

#include <memory>
#include <variant>

namespace Citron {

class RType;
using RTypePtr = std::shared_ptr<RType>;

struct RFuncReturn_ForCtor {};
struct RFuncReturn_Set
{
    RTypePtr type;
};
struct RFuncReturn_NotSet {}; // need inference
using RFuncReturn = std::variant<RFuncReturn_ForCtor, RFuncReturn_Set, RFuncReturn_NotSet>;

}

