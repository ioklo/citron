#include "QFactory.h"

#include <memory>

#include "QBlock.h"

using namespace std;

namespace Citron {

QBlock* QFactory::MakeQBlock()
{
    auto block = make_unique<QBlock>();
    auto* pBlock = block.get();
    blocks.push_back(std::move(block));
    return pBlock;
}

} // namespace Citron