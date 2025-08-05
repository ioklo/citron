#pragma once

#include <vector>

namespace Citron {

class QInst;
class QJumpInst;

// basic block
class QBlock
{
    std::vector<QInst*> insts;
    QJumpInst* terminator; // 마지막 점프 명령

public:
    void AddInst(QInst* inst);
    void SetTerminator(QJumpInst* terminator);
};

}
