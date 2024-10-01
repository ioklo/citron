#pragma once

#include <memory>

namespace Citron {

namespace SyntaxIR0Translator {

class ScopeContext
{
public:
    bool IsFailed();
};

using ScopeContextPtr = std::shared_ptr<ScopeContext>;

} // namespace SyntaxIR0Translator

} // namespace Citron