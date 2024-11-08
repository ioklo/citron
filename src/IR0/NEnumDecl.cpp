#include "NEnumDecl.h"

#include <cassert>
#include <Infra/Exceptions.h>

using namespace std;

namespace Citron
{

NEnumDecl::NEnumDecl(NTypeDeclOuterWPtr outer, RAccessor accessor, RName name, std::vector<std::string> typeParams, size_t elemCount)
    : outer(std::move(outer))
    , accessor(accessor)
    , name(std::move(name))
    , typeParams(std::move(typeParams))
{
    elems.reserve(elemCount);
}

void NEnumDecl::AddElem(std::shared_ptr<NEnumElemDecl>&& elem)
{
    elems.push_back(elem);
    elemsMap.emplace(elem->name, std::move(elem));
}

NDecl* NEnumDecl::GetNOuter()
{
    return outer.lock()->GetNDecl();
}

RMember NEnumDecl::ToRMember(const shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs)
{
    auto sharedEnumDecl = dynamic_pointer_cast<NEnumDecl>(sharedThis);
    assert(sharedEnumDecl);
    return RMember_Enum(typeArgs, sharedEnumDecl);
}

RDecl* NEnumDecl::GetROuter()
{
    return outer.lock()->GetNDecl()->GetRDecl();
}

RIdentifier NEnumDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

optional<RMember> NEnumDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    auto* normalName = get_if<RName_Normal>(&name);
    if (!normalName) return nullopt;

    // enumElem에서 검색, 같은 이름은 
    auto i = elemsMap.find(normalName->text);
    if (i == elemsMap.end()) return nullopt;

    return RMember_EnumElem(typeArgs, i->second);
}


optional<RMember> NEnumDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    // MemberVarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

} // namespace Citron