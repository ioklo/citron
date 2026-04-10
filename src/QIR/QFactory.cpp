#include "QFactory.h"

#include <memory>

#include "QBlock.h"
#include "QData.h"

using namespace std;

namespace Citron {

QData* QFactory::MakeQData(std::vector<QFuncBody>&& body)
{
    auto data = make_unique<QData>(std::move(body));
    auto* pData = data.get();
    dataList.push_back(std::move(data));
    return pData;
}

QBlock* QFactory::MakeQBlock(std::string&& debugText)
{
    auto block = make_unique<QBlock>(std::move(debugText));
    auto* pBlock = block.get();
    blocks.push_back(std::move(block));
    return pBlock;
}

} // namespace Citron