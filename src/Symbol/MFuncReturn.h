#pragma once

#include "MType.h"

namespace Citron {

struct MNoneFuncReturn {}; // for constructor
struct MConfirmedFuncReturn
{
    MType type;
};
struct MNeedInferenceFuncReturn {};
using MFuncReturn = std::variant<MNoneFuncReturn, MConfirmedFuncReturn, MNeedInferenceFuncReturn>;

}

