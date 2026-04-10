#pragma once
#include "QIRConfig.h"

#include <vector>
#include <memory>
#include <string>

namespace Citron {

class QData;
class QBlock;
struct QFuncBody;

class QFactory
{
    std::vector<std::unique_ptr<QData>> dataList;
    std::vector<std::unique_ptr<QBlock>> blocks;
    
public:
    QIR_API QData* MakeQData(std::vector<QFuncBody>&& funcBodies);
    QIR_API QBlock* MakeQBlock(std::string&& debugText);
};

using QFactoryPtr = std::shared_ptr<QFactory>;

}