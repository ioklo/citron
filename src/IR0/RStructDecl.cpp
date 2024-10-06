#include "RStructDecl.h"

namespace Citron {

RDecl* RStructDecl::GetOuter()
{
    return outer.lock()->GetDecl();
}

RIdentifier RStructDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

RDecl* RStructDecl::GetDecl()
{
    return this;
}

} // namespace Citron