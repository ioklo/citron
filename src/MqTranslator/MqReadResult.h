#pragma once
#include <variant>
#include "MqLocResult.h"

namespace Citron {

struct MqReadResult_Slot { size_t slotIndex; }; // local var
struct MqReadResult_Ptr { size_t slotIndex; };  // ptr
struct MqReadResult_ConstBool { bool value; };
struct MqReadResult_ConstInt32 { int value; };

using MqReadResult = std::variant<MqReadResult_Slot, MqReadResult_Ptr, MqReadResult_ConstBool, MqReadResult_ConstInt32>;

MqReadResult ToReadResult(MqLocResult& locResult);

//visit([](auto& result) -> ResultType {
//    using T = remove_cvref_t<decltype(result)>;
//    if constexpr (same_as<T, MqReadResult_Slot>)
//    else if constexpr (same_as<T, MqReadResult_Ptr>)
//    else if constexpr (same_as<T, MqReadResult_ConstBool>)
//    else if constexpr (same_as<T, MqReadResult_ConstInt32>)
//    else static_assert(false)
//}, *e_result);

} // namespace Citron