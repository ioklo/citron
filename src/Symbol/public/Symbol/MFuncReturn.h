#pragma once

#include <variant>

namespace Citron {

using MTypePtr = std::shared_ptr<class MType>;

struct MFuncReturn_ForCtor {}; // for ctor
struct MFuncReturn_Normal
{
    MTypePtr type;
};
using MFuncReturn = std::variant<MFuncReturn_ForCtor, MFuncReturn_Normal>;

}

