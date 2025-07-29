#pragma once

#include <variant>
#include <memory>


namespace Citron {

class MType;
using MTypePtr = std::shared_ptr<MType>;

struct MFuncReturn_ForCtor {}; // for ctor
struct MFuncReturn_Normal
{
    MTypePtr type;
};

using MFuncReturn = std::variant<MFuncReturn_ForCtor, MFuncReturn_Normal>;

}

