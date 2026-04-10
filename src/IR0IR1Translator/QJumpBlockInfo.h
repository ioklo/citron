#pragma once
#include <variant>
#include <memory>

namespace Citron {

class QBlock;
using QLazyBlockPtr = std::shared_ptr<class QLazyBlock>;

// TODO: [38] break/continue에 label 지원
struct QJumpBlockInfo_Loop { size_t labelId; QBlock* contBlock; QBlock* breakBlock; };
struct QJumpBlockInfo_Switch { size_t labelId; QBlock* breakBlock; };
struct QJumpBlockInfo_Inline { size_t labelId; QLazyBlockPtr lazyLeaveBlock; size_t leaveSlotIndex; };
using QJumpBlockInfo = std::variant<QJumpBlockInfo_Loop, QJumpBlockInfo_Switch, QJumpBlockInfo_Inline>;

} // namespace Citron
