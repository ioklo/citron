#pragma once
#include "MIRConfig.h"

#include <variant>

namespace Citron {

class RType;
class RFactory;
struct MLoc;
struct MExp;

// BC(Bitwise Copyable)/NBC(Non-bitwise Copyable) 모두 지원하는 읽기
// NOTICE: materialized struct는 MRead_Location으로 작성해야 한다
struct MRead_Exp { MExp* exp; };  // BC 전용
struct MRead_Loc { MLoc* loc; };  // BC/NBC 겸용. rvalue가 있으면 MLoc_Materialize를 사용한다

using MRead = std::variant<MRead_Exp, MRead_Loc>;

MIR_API RType* GetType(MRead& read, RFactory* rFactory);

} // namespace Citron