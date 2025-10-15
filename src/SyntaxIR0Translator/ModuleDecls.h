#pragma once

#include <vector>
#include <memory>

namespace Citron {

class RModule;

namespace SyntaxIR0Translator {

struct RModules
{
    std::vector<RModule*> modules;
};

} // namespace SyntaxIR0Translator
} // namespace Citron
