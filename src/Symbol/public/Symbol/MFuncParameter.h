#pragma once

#include <string>
#include <memory>

namespace Citron {

class MType;
using MTypePtr = std::shared_ptr<MType>;

class MFuncParameter
{
    bool bOut;
    MTypePtr type;
    std::string name;
};


}
