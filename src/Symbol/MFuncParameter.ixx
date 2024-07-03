module;
#include "Macros.h"
export module Citron.Symbols:MFuncParameter;

import "std.h";
import :MType;

namespace Citron {

export class MFuncParameter
{
    bool bOut;
    MType type;
    std::string name;
};


}
