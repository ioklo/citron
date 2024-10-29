#include "NStructDecl.h"

namespace Citron {

NDecl* NStructDecl::GetOuter()
{
    return outer.lock()->GetDecl();
}

RIdentifier NStructDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

NDecl* NStructDecl::GetDecl()
{
    return this;
}

} // namespace Citron