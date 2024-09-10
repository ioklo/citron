#pragma once

#include <string>
#include <memory>

namespace Citron {

using RTypePtr = std::shared_ptr<class RType>;

struct RFuncParameter
{
    bool bOut;
    RTypePtr type;
    std::string name;
};


}
