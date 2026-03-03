#pragma once
#include "MIRConfig.h"
#include <vector>
#include <variant>
#include "MMoveSource.h"

namespace Citron {

class RType;
class RFactory;

struct MExp;
struct MInitExp;

struct MCreate_Bitwise { MExp* exp; };
struct MCreate_Init { MInitExp* initExp; };

using MCreate = std::variant<MCreate_Bitwise, MCreate_Init>;

MIR_API RType* GetType(MCreate& create, RFactory* rFactory);

} // Citron