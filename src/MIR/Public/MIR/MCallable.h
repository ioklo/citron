#pragma once
#include "MIRConfig.h"

#include <variant>

namespace Citron {

class RType;
class RFactory;
class RGlobalFuncDecl;
class RTypeArguments;
class RClassFuncDecl;
class RClassCtorDecl;
class RStructFuncDecl;
class RStructCtorDecl;
class RLambdaDecl;
class RFuncDecl;
struct MLoc;

struct MCallable
{
    RFuncDecl* decl;
    RTypeArguments* typeArgs;
    MLoc* o_instance;
};

} // namespace Citron
