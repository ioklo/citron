#pragma once

#include <string>
#include <memory>

namespace Citron {

using RTypePtr = std::shared_ptr<class RType>;

class RFuncParameter
{
    bool bOut;
    RTypePtr type;
    std::string name;
};


}
