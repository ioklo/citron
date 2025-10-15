#include "NInterfaceDecl.h"

#include "Infra/Exceptions.h"

using namespace std;

namespace Citron {

NDecl* NInterfaceDecl::GetNOuter()
{
    return outer->GetNDecl();
}

RMember NInterfaceDecl::ToRMember(RTypeArguments* typeArgs)
{
    throw NotImplementedException();
}

RDecl* NInterfaceDecl::GetROuter()
{
    return outer->GetNDecl()->GetRDecl();
}

RIdentifier NInterfaceDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

optional<RMember> NInterfaceDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    throw NotImplementedException();
}

optional<RMember> NInterfaceDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory)
{
    throw NotImplementedException();
}

} // namespace Citron