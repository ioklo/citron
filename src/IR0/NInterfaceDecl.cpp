#include "NInterfaceDecl.h"

#include "Infra/Exceptions.h"

using namespace std;

namespace Citron {

NDecl* NInterfaceDecl::GetNOuter()
{
    return outer.lock()->GetNDecl();
}

RDecl* NInterfaceDecl::GetROuter()
{
    return outer.lock()->GetNDecl()->GetRDecl();
}

RIdentifier NInterfaceDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

optional<RMember> NInterfaceDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    throw NotImplementedException();
}

optional<RMember> NInterfaceDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    throw NotImplementedException();
}

} // namespace Citron