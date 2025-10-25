#include "RNamespaceDeclGroup.h"

namespace Citron {

void RNamespaceDeclGroup::Add(RNamespaceDecl* decl)
{
    decls.push_back(decl);
}

} // namespace Citron