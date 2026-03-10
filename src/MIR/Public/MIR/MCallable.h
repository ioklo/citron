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
struct MLoc;

struct MCallable_GlobalFunc { RGlobalFuncDecl* decl; RTypeArguments* typeArgs; }; // F();
struct MCallable_ClassFunc { RClassFuncDecl* decl; RTypeArguments* typeArgs; MLoc* instance; }; // c.F();
struct MCallable_StructFunc { RStructFuncDecl* decl; RTypeArguments* typeArgs; MLoc* instance; }; // s.F();
struct MCallable_Lambda { RLambdaDecl* decl; RTypeArguments* typeArgs; MLoc* callable; }; // f(2, 3) 

using MCallable = std::variant<MCallable_GlobalFunc, MCallable_ClassFunc, MCallable_StructFunc, MCallable_Lambda>;

MIR_API RType* GetType(MCallable& call);

} // namespace Citron
