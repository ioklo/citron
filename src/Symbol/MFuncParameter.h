#pragma once
#include "SymbolMacros.h"

namespace Citron {

using MTypePtr = std::shared_ptr<class MType>;

class MFuncParameter
{
    bool bOut;
    MTypePtr type;
    std::string name;
};


}
