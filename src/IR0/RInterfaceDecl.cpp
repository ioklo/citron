#include "RInterfaceDecl.h"

namespace Citron {

RDecl* RInterfaceDecl::GetOuter()
{
    return outer.lock()->GetDecl();
}

RIdentifier RInterfaceDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

} // namespace Citron