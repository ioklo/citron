#pragma once

#include <string>

#include "RNames.h"

namespace Citron {

class RType;
class RTypeArguments;
class RFactory;

struct RFuncParameter
{
    bool bOut;
    RType* type; // 람다의 경우 지정이 안될 수 있다
    RName name;

    RFuncParameter Apply(RTypeArguments& typeArgs, RFactory& typeFactory);
};


}
