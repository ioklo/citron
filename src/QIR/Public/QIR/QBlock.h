#pragma once

#include <vector>
#include <cassert>
#include <optional>
#include "QInsts.h"

namespace Citron {

// basic block
class QBlock
{
    std::vector<QInst> insts;
    std::optional<QInst> oTerminator; // 마지막 점프 명령

public:
    QBlock()
    {
    }

    void AddInst(QInst&& inst)
    {
        assert(!this->oTerminator);
        insts.push_back(std::move(inst));
    }

    void SetTerminator(QJumpInst&& terminator)
    {
        assert(!this->oTerminator);
        visit([this](auto&& t) {this->oTerminator = std::move(t); }, std::move(terminator));
    }

    QInst& GetInst(size_t index)
    {
        assert(index < insts.size() + 1);

        if (index < insts.size())
            return insts[index];
        else
            return *oTerminator;
    }
};

}
