#pragma once
#include <variant>
#include <memory>

namespace Citron {

class QBlock;
using MqLazyBlockPtr = std::shared_ptr<class MqLazyBlock>;

struct MqJumpBlockInfo_Loop { size_t labelId; QBlock* contBlock; QBlock* breakBlock; };
struct MqJumpBlockInfo_Switch { size_t labelId; QBlock* breakBlock; };
struct MqJumpBlockInfo_Inline { size_t labelId; MqLazyBlockPtr lazyLeaveBlock; size_t leaveSlotIndex; };
using MqJumpBlockInfo = std::variant<MqJumpBlockInfo_Loop, MqJumpBlockInfo_Switch, MqJumpBlockInfo_Inline>;

} // namespace Citron
