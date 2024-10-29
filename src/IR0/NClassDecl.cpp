#include "NClassDecl.h"

namespace Citron {

NDecl* NClassDecl::GetOuter()
{
    return outer.lock()->GetDecl();
}

RIdentifier NClassDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

NDecl* NClassDecl::GetDecl()
{
    return this;
}

} // namespace Citron