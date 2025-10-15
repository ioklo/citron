#include "NEnumDecl.h"

#include <cassert>
#include "Infra/Exceptions.h"

using namespace std;

namespace Citron
{

NEnumDecl::NEnumDecl(NTypeDeclOuter* outer, RAccessor accessor, RName name, std::vector<std::string> typeParams, size_t elemCount)
    : outer(outer)
    , accessor(accessor)
    , name(move(name))
    , typeParams(move(typeParams))
{
    elems.reserve(elemCount);
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

optional<RMember> NEnumDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    auto* normalName = get_if<RName_Normal>(&name);
    if (!normalName) return nullopt;

    // enumElem에서 검색, 같은 이름은 
    auto i = elemsMap.find(normalName->text);
    if (i == elemsMap.end()) return nullopt;

    return RMember_EnumElem(typeArgs, i->second);
}


optional<RMember> NEnumDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory)
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

} // namespace Citron