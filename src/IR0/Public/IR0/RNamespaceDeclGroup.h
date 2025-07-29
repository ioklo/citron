#pragma once

#include <memory>
#include <vector>

namespace Citron {

class RNamespaceDecl;

class RNamespaceDeclGroup
{
    std::vector<std::shared_ptr<RNamespaceDecl>> decls; // 같은 이름들의 namespace

public:
    void Add(const std::shared_ptr<RNamespaceDecl>& decl);
};

using RNamespaceDeclGroupPtr = std::shared_ptr<RNamespaceDeclGroup>;

}
