#pragma once

#include <memory>
#include <variant>

namespace Citron {

using RTypePtr = std::shared_ptr<class RType>;

struct RNoneFuncReturn {}; // for constructor
struct RConfirmedFuncReturn
{
    RTypePtr type;
};
struct RNeedInferenceFuncReturn {};
using RFuncReturn = std::variant<RNoneFuncReturn, RConfirmedFuncReturn, RNeedInferenceFuncReturn>;

}

