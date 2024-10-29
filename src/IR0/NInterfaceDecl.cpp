#include "NInterfaceDecl.h"

namespace Citron {

NDecl* NInterfaceDecl::GetOuter()
{
    return outer.lock()->GetDecl();
}

RIdentifier NInterfaceDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

} // namespace Citron