#pragma once

#include <variant>

namespace Citron {

struct MLoc;
struct MExp;

// BC/NBC 모두 지원하는 읽기
// NOTICE: materialized struct는 MRead_Location으로 작성해야 한다
struct MRead_Value { MExp* exp; }; // BC전용. loc이 와도 MOperand_Value(MExp_Load(loc)) 으로 작성한다
struct MRead_Location { MLoc* loc; };  // NBC 전용, rvalue가 있으면 MLoc_Materialize를 사용한다

using MRead = std::variant<MRead_Value, MRead_Location>;

} // namespace Citron