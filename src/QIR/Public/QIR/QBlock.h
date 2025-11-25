#pragma once

#include <vector>
#include <string>
#include <cassert>
#include <optional>
#include "QInsts.h"

namespace Citron {

// basic block
class QBlock
{
    std::string debugText;
    std::vector<QInst> insts;

    friend class QPrinter;

public:
    QBlock(std::string&& debugText)
        : debugText(std::move(debugText))
    {
    }

    void EmitInst(QInst&& inst)
    {
        insts.push_back(std::move(inst));
    }

    size_t GetInstCount()
    {
        return insts.size();
    }
    
    QInst& GetInst(size_t index)
    {
        return insts[index];
    }
};

}
