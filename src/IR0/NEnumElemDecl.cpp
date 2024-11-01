#include "NEnumElemDecl.h"
#include <cassert>
#include "NEnumDecl.h"

using namespace std;

namespace Citron {

NEnumElemDecl::NEnumElemDecl(weak_ptr<NEnumDecl> _enum, string name, size_t memberVarCount)
    : _enum(move(_enum)), name(move(name))
{
    memberVars.reserve(memberVarCount);
}

void NEnumElemDecl::AddMemberVar(const std::shared_ptr<NEnumElemMemberVarDecl>& memberVar)
{
    memberVars.push_back(memberVar);
    memberVarsMap.emplace(memberVar->name, std::move(memberVar));
}

NDecl* NEnumElemDecl::GetOuter()
{
    return _enum.lock().get();
}

RIdentifier NEnumElemDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

optional<RMember> NEnumElemDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    return GetMemberVar(typeArgs, name);
}

optional<RMember_EnumElemMemberVar> NEnumElemDecl::GetMemberVar(const RTypeArgumentsPtr& typeArgs, const RName& name)
{
    auto* normalName = get_if<RName_Normal>(&name);
    if (!normalName) return nullopt;

    auto i = memberVarsMap.find(normalName->text);
    if (i == memberVarsMap.end()) return nullopt;

    return RMember_EnumElemMemberVar(typeArgs, i->second);
}

RMember NEnumElemDecl::ToRMember(const shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs)
{
    auto sharedEnumElemDecl = dynamic_pointer_cast<NEnumElemDecl>(sharedThis);
    assert(sharedEnumElemDecl);
    return RMember_EnumElem(typeArgs, sharedEnumElemDecl);
}

} // namespace Citron