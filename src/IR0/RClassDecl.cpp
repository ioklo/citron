#include "RClassDecl.h"

namespace Citron {

RDecl* RClassDecl::GetOuter()
{
    return outer.lock()->GetDecl();
}

RIdentifier RClassDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

RDecl* RClassDecl::GetDecl()
{
    return this;
}

} // namespace Citron