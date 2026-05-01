#pragma once
#include <memory>
#include <string>

namespace Citron {

class QBlock;
class MqBodyContext;

class MqLazyBlock
{   
    std::string debugName;
    QBlock* o_block;

public:
    MqLazyBlock(const std::string& debugName) : debugName{debugName}, o_block{nullptr} {}
    bool HasBlock() { return o_block != nullptr; }
    QBlock* GetBlock(MqBodyContext* context);
};

using MqLazyBlockPtr = std::shared_ptr<MqLazyBlock>;

} // namespace Citron