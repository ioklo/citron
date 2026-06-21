#pragma once
#include "RSymbolConfig.h"
#include <variant>

namespace Citron {

class RType;
class RFactory;

struct RFuncReturn_None {}; // for ctor, dtor
struct RFuncReturn_Normal { RType* type; };
struct RFuncReturn_NotSet {}; // need inference
using RFuncReturn = std::variant<RFuncReturn_None, RFuncReturn_Normal, RFuncReturn_NotSet>;

RSYMBOL_API RType* GetType(RFuncReturn& funcRet, RFactory* rFactory);

}

