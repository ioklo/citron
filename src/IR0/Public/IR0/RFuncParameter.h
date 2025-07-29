#pragma once

#include <string>
#include <memory>

#include "RNames.h"

namespace Citron {

class RType;
using RTypePtr = std::shared_ptr<RType>;

class RTypeArguments;
class RTypeFactory;

struct RFuncParameter
{
    bool bOut;
    RTypePtr type; // 람다의 경우 지정이 안될 수 있다
    RName name;

    RFuncParameter Apply(RTypeArguments& typeArgs, RTypeFactory& typeFactory);
};


}
