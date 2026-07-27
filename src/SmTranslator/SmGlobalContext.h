#pragma once
#include <memory>

namespace Citron {

class SmGlobalContext
{
    std::optional<RTypeRes> ResolveTypeIdentifier(RTypeArguments* typeArgs, InRef<RName> name);
};

using SmGlobalContextPtr = std::shared_ptr<SmGlobalContext>;

} // namespace Citron


