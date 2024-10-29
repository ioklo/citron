#include "NNamespaceDecl.h"

namespace Citron {

NNamespaceDecl::NNamespaceDecl(NTopLevelDeclOuterWPtr outer, std::string name)
    : outer(outer), name(name)
{

}

NDecl* NNamespaceDecl::GetOuter()
{
    return outer.lock()->GetDecl();
}

RIdentifier NNamespaceDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

NDecl* NNamespaceDecl::GetDecl()
{
    return this;
}

} // namespace Citron