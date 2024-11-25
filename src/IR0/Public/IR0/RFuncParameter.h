#pragma once

#include <string>
#include <memory>

namespace Citron {

using RTypePtr = std::shared_ptr<class RType>;

struct RFuncParameter
{
    bool bOut;
    RTypePtr type; // 람다의 경우 지정이 안될 수 있다
    RName name;
};


}
