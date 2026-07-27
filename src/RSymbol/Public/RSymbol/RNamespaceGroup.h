#pragma once
#include <vector>

namespace Citron {

class RNamespace;

class RNamespaceGroup
{
    std::vector<RNamespace*> namespaces;

public:
    RNamespaceGroup(std::vector<RNamespace*>&& namespaces)
        : namespaces{std::move(namespaces)}
    {
    }
};


} // namespace Citron
