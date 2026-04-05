#pragma once
#include <variant>

namespace Citron {

class QBlock;

// TODO: [38] break/continue에 label 지원
struct QJumpBlockInfo_Loop { size_t labelId; QBlock* contBlock; QBlock* breakBlock; };
struct QJumpBlockInfo_Switch { size_t labelId; QBlock* breakBlock; };
using QJumpBlockInfo = std::variant<QJumpBlockInfo_Loop, QJumpBlockInfo_Switch>;

} // namespace Citron
