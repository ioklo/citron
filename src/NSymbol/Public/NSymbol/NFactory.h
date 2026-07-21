#pragma once
#include "NSymbolConfig.h"

#include <vector>
#include <memory>
#include <string>

namespace Citron {

using RFactoryPtr = std::shared_ptr<class RFactory>;
struct NStructInfo;
struct NImplTraitFunc;

struct NFactoryPrivateData;

class NFactory
{
    RFactoryPtr rFactory;
    std::unique_ptr<NFactoryPrivateData> privateData;

public:
    NSYMBOL_API NFactory(RFactoryPtr& rFactory);
    NSYMBOL_API ~NFactory();

    NSYMBOL_API NStructInfo* MakeNStructInfo();
    NSYMBOL_API NImplTraitFunc* MakeNImplTraitFunc();
};

using NFactoryPtr = std::shared_ptr<NFactory>;

} // namespace Citron