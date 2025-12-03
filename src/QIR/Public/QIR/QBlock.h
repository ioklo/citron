#pragma once

#include <vector>
#include <string>
#include <span>

#include "QInsts.h"

namespace Citron {

// basic block
class QBlock
{
    std::string debugText;
    std::vector<QInst> insts;
    size_t index;

    friend class QPrinter;

public:
    QBlock(size_t index, std::string&& debugText)
        : index{index}, debugText(std::move(debugText))
    {
    }

    size_t GetIndex() { return index; }
    std::string_view GetName() { return debugText; }

    void EmitInst(QInst&& inst)
    {
        insts.push_back(std::move(inst));
    }

    std::span<QInst> GetInsts() { return insts; }

    QInst& GetInst(size_t index)
    {
        return insts[index];
    }
};

}
