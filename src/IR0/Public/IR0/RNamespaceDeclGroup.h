#pragma once

#include <vector>

namespace Citron {

class RNamespaceDecl;

class RNamespaceDeclGroup
{
    std::vector<RNamespaceDecl*> decls; // 같은 이름들의 namespace

public:
    void Add(RNamespaceDecl* decl);
};

}
