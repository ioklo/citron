#pragma once
#include "SymbolMacros.h"
#include <string>

namespace Citron {

using MTypePtr = std::shared_ptr<class MType>;

class MFuncParameter
{
    bool bOut;
    MTypePtr type;
    std::string name;
};


}
