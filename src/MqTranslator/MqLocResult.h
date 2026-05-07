#pragma once
#include <variant>

namespace Citron {

struct MqLocResult_Slot { size_t slotIndex; }; // local var
struct MqLocResult_Ptr { size_t slotIndex; }; // ptr

using MqLocResult = std::variant<MqLocResult_Slot, MqLocResult_Ptr>;

} // namespace Citron