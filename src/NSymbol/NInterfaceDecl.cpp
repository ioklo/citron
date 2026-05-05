#include "NInterfaceDecl.h"

#include "Infra/Exceptions.h"

#include "NTypeParamDecl.h"

using namespace std;

namespace Citron {
NInterfaceDecl::NInterfaceDecl(NTypeDeclOuter* outer, RAccessor accessor, RName&& name)
    : outer{outer}, accessor{accessor}, name{move(name)}
{
}

NDecl* NInterfaceDecl::GetNOuter()
{
    return outer->GetNDecl();
}

RDeclRes NInterfaceDecl::ToRDeclRes(RTypeArguments* typeArgs)
{
    throw NotImplementedException();
}

RDecl* NInterfaceDecl::GetROuter()
{
    return outer->GetNDecl()->GetRDecl();
}

RIdentifier NInterfaceDecl::GetIdentifier()
{
    return RIdentifier{ name, NGenericsComponent::GetTypeParamCount(), {}};
}

RTypeDecl* NInterfaceDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    return NGenericsComponent::GetTypeMember(name, typeParamCount);
}

optional<RDeclRes> NInterfaceDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    throw NotImplementedException();
}

optional<RDeclRes> NInterfaceDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = NGenericsComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    throw NotImplementedException();
}

RType* NInterfaceDecl::GetOpenType()
{
    throw NotImplementedException{};
}

} // namespace Citron