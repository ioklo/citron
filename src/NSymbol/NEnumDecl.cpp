#include "NEnumDecl.h"

#include <cassert>
#include "Infra/Exceptions.h"

#include "NTypeParamDecl.h"

using namespace std;

namespace Citron
{
NEnumDecl::NEnumDecl(NTypeDeclOuter* outer, RAccessor accessor, const RName& name)
    : outer{outer}
    , accessor{accessor}
    , name{name}
{}

void NEnumDecl::AddElem(NEnumElemDecl* elem)
{
    elems.push_back(elem);
    elemsMap.emplace(elem->name, elem);
}

NDecl* NEnumDecl::GetNOuter()
{
    return outer->GetNDecl();
}

RDeclRes NEnumDecl::ToRDeclRes(RTypeArguments* typeArgs)
{   
    return RDeclRes_Enum(typeArgs, this);
}

RDecl* NEnumDecl::GetROuter()
{
    return outer->GetNDecl()->GetRDecl();
}

RIdentifier NEnumDecl::GetIdentifier()
{
    return RIdentifier { name, NGenericsComponent::GetTypeParamCount(), {} };
}

RTypeDecl* NEnumDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    if (RTypeDecl* typeDecl = NGenericsComponent::GetTypeMember(name, typeParamCount))
        return typeDecl;

    auto i = elemsMap.find(name);
    if (i != elemsMap.end())
        return i->second->GetRTypeDecl();

    return nullptr;
}

optional<RDeclRes> NEnumDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    // enumElem에서 검색, 같은 이름은 
    auto i = elemsMap.find(name);
    if (i == elemsMap.end()) return nullopt;

    return RDeclRes_EnumElem(typeArgs, i->second);
}


optional<RDeclRes> NEnumDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = NGenericsComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

} // namespace Citron