#pragma once
#include "MIRConfig.h"

#include <variant>
#include "RSymbol/RFuncDecl.h"

namespace Citron {

class RTypeArguments;
struct MLoc;

struct MCallable
{
    RFuncDecl* decl;
    RTypeArguments* typeArgs;
    MLoc* o_instance;
};

} // namespace Citron
