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

struct MCallable_GlobalFunc { RGlobalFuncDecl* decl; RTypeArguments* typeArgs; }; // F();
struct MCallable_ClassCtor { RClassCtorDecl* decl; RTypeArguments* typeArgs; MLoc* instance; }; // constructor에서 base ctor호출시 사용
struct MCallable_ClassFunc { RClassFuncDecl* decl; RTypeArguments* typeArgs; MLoc* instance; }; // c.F();
struct MCallable_StructCtor { RStructCtorDecl* decl; RTypeArguments* typeArgs; MLoc* instance; }; // constructor에서 base ctor호출시 사용
struct MCallable_StructFunc { RStructFuncDecl* decl; RTypeArguments* typeArgs; MLoc* instance; }; // s.F();
struct MCallable_Lambda { RLambdaDecl* decl; RTypeArguments* typeArgs; MLoc* callable; }; // f(2, 3) 

using MCallable = std::variant<MCallable_GlobalFunc, MCallable_ClassFunc, MCallable_StructFunc, MCallable_Lambda>;

MIR_API RType* GetType(MCallable& call);
MIR_API RFuncDecl* GetRFuncDecl(MCallable& call);

} // namespace Citron
