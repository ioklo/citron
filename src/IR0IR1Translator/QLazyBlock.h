#pragma once
#include <memory>
#include <string>

namespace Citron {

class QBlock;
class QBodyContext;

class QLazyBlock
{   
    std::string debugName;
    QBlock* o_block;

public:
    QLazyBlock(const std::string& debugName) : debugName{debugName}, o_block{nullptr} {}
    bool HasBlock() { return o_block != nullptr; }
    QBlock* GetBlock(QBodyContext* context);
};

using QLazyBlockPtr = std::shared_ptr<QLazyBlock>;

} // namespace Citron