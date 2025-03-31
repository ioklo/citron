export module Citron.RDecls:RNamespaceDeclGroup;

import <memory>;
import <vector>;

namespace Citron {

export class RNamespaceDecl;

export class RNamespaceDeclGroup
{
    std::vector<std::shared_ptr<RNamespaceDecl>> decls; // 같은 이름들의 namespace

public:
    void Add(const std::shared_ptr<RNamespaceDecl>& decl);
};

export using RNamespaceDeclGroupPtr = std::shared_ptr<RNamespaceDeclGroup>;

}
