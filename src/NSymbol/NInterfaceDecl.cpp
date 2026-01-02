#include "NInterfaceDecl.h"

#include "Infra/Exceptions.h"

#include "NTypeParamDecl.h"

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

RTypeParamDecl* NInterfaceDecl::GetTypeParam(size_t index)
{
    return typeParams[index];
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

optional<RMember> NInterfaceDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    throw NotImplementedException();
}

} // namespace Citron