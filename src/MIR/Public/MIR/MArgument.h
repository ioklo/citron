#pragma once

#include <variant>
#include "MCreate.h"

namespace Citron {

struct MExp;
class MLoc;

struct MArgument_Create { MCreate create; };
struct MArgument_Loc { MLoc* loc; };
struct MArgument_Move { MMoveSource src; };
struct MArgument_Params { MExp* exp; size_t elemCount; };

using MArgument = std::variant<MArgument_Create, MArgument_Loc, MArgument_Move, MArgument_Params>; 

}