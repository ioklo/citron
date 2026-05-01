#include "MqLazyBlock.h"
#include "MqBodyContext.h"

namespace Citron {

QBlock* MqLazyBlock::GetBlock(MqBodyContext* context)
{
    if (!o_block) 
        o_block = context->AddBlock(std::string{debugName});

    return o_block;
}

} // namespace Citron