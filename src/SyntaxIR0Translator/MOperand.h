#pragma once

#include <variant>

namespace Citron {

class MLoc;
class MExp;

struct MOperand_Loc { MLoc* loc; };
struct MOperand_Exp { MExp* exp; };

using MOperand = std::variant<MOperand_Loc, MOperand_Exp>;

} // namespace Citron