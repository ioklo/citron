#pragma once

#include <string>
#include <memory>

#include "RNames.h"

namespace Citron {

class RType;
class RTypeArguments;
class RTypeFactory;

struct RFuncParameter
{
    bool bOut;
    RType* type; // 람다의 경우 지정이 안될 수 있다
    RName name;

    RFuncParameter Apply(RTypeArguments& typeArgs, RTypeFactory& typeFactory);
};


}
