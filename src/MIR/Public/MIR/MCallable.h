#pragma once
#include "MIRConfig.h"

#include <variant>

namespace Citron {

class RType;
class RFactory;
class RGlobalFuncDecl;
class RTypeArguments;
class RClassFuncDecl;
class RStructFuncDecl;
class RLambdaDecl;
class MLoc;

struct MCall_GlobalFunc { RGlobalFuncDecl* decl; RTypeArguments* typeArgs; }; // F();
struct MCall_ClassFunc { RClassFuncDecl* decl; RTypeArguments* typeArgs; MLoc* instance; }; // c.F();
struct MCall_StructFunc { RStructFuncDecl* decl; RTypeArguments* typeArgs; MLoc* instance; }; // s.F();
struct MCall_Lambda { RLambdaDecl* decl; RTypeArguments* typeArgs; MLoc* callable; }; // f(2, 3) 

using MCallable = std::variant<MCall_GlobalFunc, MCall_ClassFunc, MCall_StructFunc, MCall_Lambda>;

MIR_API RType* GetType(MCallable& call);

} // namespace Citron
