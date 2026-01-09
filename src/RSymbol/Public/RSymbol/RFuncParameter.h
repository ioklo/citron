#pragma once
#include "RSymbolConfig.h"
#include <string>

#include "RNames.h"

namespace Citron {

class RType;
class RTypeArguments;
class RFactory;

enum class RFuncParameterKind
{
    Normal,
    In,
    Move,
    Forward,
    Out,
    Params,
    Init, // memberwise ctor용, ref 도 활성화 하도록
};

struct RFuncParameter
{
    RFuncParameterKind kind;
    bool bRef;
    RType* type; // 람다의 경우 지정이 안될 수 있다
    RName name;

    RSYMBOL_API RFuncParameter Apply(RTypeArguments& typeArgs);
};


}
