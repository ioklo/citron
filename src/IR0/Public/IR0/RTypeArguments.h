#pragma once

#include "IR0Config.h"

#include <vector>
#include <memory>

namespace Citron {

using RTypePtr = std::shared_ptr<class RType>;

class RTypeFactory;
class RTypeArgumentsFactory;

class RTypeArguments
{
    std::vector<RTypePtr> items; 

private:
    friend RTypeFactory;
    RTypeArguments(const std::vector<RTypePtr>& items);

public:
    IR0_API size_t GetCount();
    IR0_API const RTypePtr& Get(int i);
    IR0_API std::shared_ptr<RTypeArguments> Apply(RTypeArguments& typeArgs, RTypeFactory& typeFactory);
};

using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

} // namespace Citron