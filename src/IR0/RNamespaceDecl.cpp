#include "RNamespaceDecl.h"

namespace Citron {

RNamespaceDecl::RNamespaceDecl(RTopLevelDeclOuterPtr outer, std::string name)
    : outer(outer), name(name)
{

}

RDecl* RNamespaceDecl::GetOuter()
{
    return outer.lock()->GetDecl();
}

RIdentifier RNamespaceDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

RDecl* RNamespaceDecl::GetDecl()
{
    return this;
}

} // namespace Citron