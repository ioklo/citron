#pragma once

#include <variant>

namespace Citron {

using MTypePtr = std::shared_ptr<class MType>;

struct MNoneFuncReturn {}; // for constructor
struct MConfirmedFuncReturn
{
    MTypePtr type;
};
struct MNeedInferenceFuncReturn {};
using MFuncReturn = std::variant<MNoneFuncReturn, MConfirmedFuncReturn, MNeedInferenceFuncReturn>;

}

