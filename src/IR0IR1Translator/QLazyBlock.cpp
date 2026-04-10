#include "QLazyBlock.h"
#include "QBodyContext.h"

namespace Citron {

QBlock* QLazyBlock::GetBlock(QBodyContext* context)
{
    if (!o_block) 
        o_block = context->AddBlock(std::string{debugName});

    return o_block;
}

} // namespace Citron