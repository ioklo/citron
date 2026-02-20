#pragma once
#include "MIRConfig.h"
#include <vector>
#include <variant>
#include "MMoveSource.h"

namespace Citron {

class RStructCtorDecl;
class RTypeArguments;
class RType;

class MExp;
class MLoc;

using MArgument = std::variant<struct MArgument_Create, struct MArgument_Loc, struct MArgument_Move, struct MArgument_Params>;

struct MCreate_Bitwise { MExp* exp; }; // rvo-call을 제외한 나머지 bitwise exp
struct MCreate_StructCopyCtor { RType* type;  RStructCtorDecl* decl; RTypeArguments* typeArgs; MLoc* src; };
struct MCreate_StructMoveCtor { RType* type; RStructCtorDecl* decl; RTypeArguments* typeArgs; MMoveSource src; };
struct MCreate_StructCtor { RType* type; RStructCtorDecl* decl; RTypeArguments* typeArgs; std::vector<MArgument> args; };
struct MCreate_RVO { MExp* callExp; }; // bitwise, struct모두 사용. exp는 MExp_Call* 중 하나. => exp가 제대로 들어왔는지 체크 필요

using MCreate = std::variant<MCreate_Bitwise, MCreate_StructCopyCtor, MCreate_StructMoveCtor, MCreate_StructCtor, MCreate_RVO>;

MIR_API RType* GetType(MCreate& create);

} // Citron