#pragma once

#include <variant>

namespace Citron {

class MLoc;
class MExp;

// 읽기 전용
// NOTICE: materialized struct는 MOperand_Loc으로 작성해야 한다
// bitwise-copyable expression만 MOperand_Exp로 만들 수 있다
struct MOperand_Loc { MLoc* loc; }; 
struct MOperand_Exp { MExp* exp; }; 

using MOperand = std::variant<MOperand_Loc, MOperand_Exp>;

} // namespace Citron