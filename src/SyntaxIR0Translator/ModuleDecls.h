#pragma once

#include <vector>
#include <memory>

namespace Citron {

class RModule;

struct RModules
{
    std::vector<RModule*> modules;
};

} // namespace Citron
