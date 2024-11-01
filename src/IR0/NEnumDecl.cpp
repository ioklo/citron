#include "NEnumDecl.h"
#include <cassert>

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

NDecl* NEnumDecl::GetOuter()
{
    return outer.lock()->GetDecl();
}

RIdentifier NEnumDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

RMember NEnumDecl::ToRMember(const shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs)
{
    auto sharedEnumDecl = dynamic_pointer_cast<NEnumDecl>(sharedThis);
    assert(sharedEnumDecl);
    return RMember_Enum(typeArgs, sharedEnumDecl);
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


} // namespace Citron