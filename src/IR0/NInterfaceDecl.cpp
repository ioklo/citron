#include "NInterfaceDecl.h"
#include <Infra/Exceptions.h>

using namespace std;

namespace Citron {

NDecl* NInterfaceDecl::GetOuter()
{
    return outer.lock()->GetDecl();
}

RIdentifier NInterfaceDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

optional<RMember> NInterfaceDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    throw NotImplementedException();
}

} // namespace Citron