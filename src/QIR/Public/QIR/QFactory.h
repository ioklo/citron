#pragma once
#include "QIRConfig.h"

#include <vector>
#include <memory>

namespace Citron {

class QBlock;

class QFactory
{
    std::vector<std::unique_ptr<QBlock>> blocks;

public:
    QIR_API QBlock* MakeQBlock();
};

using QFactoryPtr = std::shared_ptr<QFactory>;

}