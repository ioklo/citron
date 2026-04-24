#pragma once
#include "RSymbolConfig.h"
#include <variant>

namespace Citron {

class RType;
class RFactory;

struct RFuncReturn_ForCtor {};
struct RFuncReturn_Set
{
    RType* type;
};
struct RFuncReturn_NotSet {}; // need inference
using RFuncReturn = std::variant<RFuncReturn_ForCtor, RFuncReturn_Set, RFuncReturn_NotSet>;

RSYMBOL_API RType* GetType(RFuncReturn& funcRet, RFactory* rFactory);

}

