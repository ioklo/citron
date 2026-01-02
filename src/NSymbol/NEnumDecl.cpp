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
    , name{move(name)}
{}

void NEnumDecl::InitTypeParams(std::vector<NTypeParamDecl*>&& typeParams)
{
    this->typeParams = move(typeParams);
}

void NEnumDecl::AddElem(NEnumElemDecl* elem)
{
    elems.push_back(elem);
    elemsMap.emplace(elem->name, elem);
}

NDecl* NEnumDecl::GetNOuter()
{
    return outer->GetNDecl();
}

RMember NEnumDecl::ToRMember(RTypeArguments* typeArgs)
{   
    return RMember_Enum(typeArgs, this);
}

RDecl* NEnumDecl::GetROuter()
{
    return outer->GetNDecl()->GetRDecl();
}

RIdentifier NEnumDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

RTypeParamDecl* NEnumDecl::GetTypeParam(size_t index)
{
    return typeParams[index];
}

RTypeDecl* NEnumDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    // TODO: [26] typeParams에서도 검색 (NTypeParamDecl, RType_TypeVar 추가 필요)

    auto i = elemsMap.find(name);
    if (i != elemsMap.end())
        return i->second->GetRTypeDecl();

    return nullptr;
}

optional<RMember> NEnumDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    // enumElem에서 검색, 같은 이름은 
    auto i = elemsMap.find(name);
    if (i == elemsMap.end()) return nullopt;

    return RMember_EnumElem(typeArgs, i->second);
}


optional<RMember> NEnumDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

} // namespace Citron