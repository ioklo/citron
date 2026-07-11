#pragma once
#include "NSymbolConfig.h"

#include <vector>
#include <memory>
#include <string>

namespace Citron {

using RFactoryPtr = std::shared_ptr<class RFactory>;

class NFactory
{
    RFactoryPtr rFactory;

public:
    NSYMBOL_API NFactory(RFactoryPtr& rFactory);
    NSYMBOL_API ~NFactory();
};

using NFactoryPtr = std::shared_ptr<NFactory>;

} // namespace Citron