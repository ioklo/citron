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

RTypeDecl* NInterfaceDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    // TODO: [26] typeParams에서도 검색 (NTypeParamDecl, RType_TypeVar 추가 필요)
    return nullptr;
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