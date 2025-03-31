module Citron.RDecls:RNamespaceDeclGroup;

namespace Citron {

void RNamespaceDeclGroup::Add(const std::shared_ptr<RNamespaceDecl>& decl)
{
    decls.push_back(decl);
}

} // namespace Citron