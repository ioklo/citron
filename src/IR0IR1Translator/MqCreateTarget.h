#pragma once
#include <variant>

namespace Citron {

struct MqCreateTarget_Discard {};
struct MqCreateTarget_Slot { size_t slotIndex; };
struct MqCreateTarget_Ptr { size_t slotIndex; };
struct MqCreateTarget_DirectReturn {};

using MqCreateTarget = std::variant<MqCreateTarget_Discard, MqCreateTarget_Slot, MqCreateTarget_Ptr, MqCreateTarget_DirectReturn>;

} // namespace Citron
