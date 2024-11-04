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

RMember NEnumElemDecl::ToRMember(const shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs)
{
    auto sharedEnumElemDecl = dynamic_pointer_cast<NEnumElemDecl>(sharedThis);
    assert(sharedEnumElemDecl);
    return RMember_EnumElem(typeArgs, sharedEnumElemDecl);
}

RDecl* NEnumElemDecl::GetROuter()
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

size_t NEnumElemDecl::GetMemberVarCount()
{
    return memberVars.size();
}

vector<RFuncParameter> NEnumElemDecl::GetUnboundConstructorParams()
{
    vector<RFuncParameter> result;

    result.reserve(memberVars.size());
    for (auto& memberVar : memberVars)
        result.emplace_back(/*bOut*/ false, memberVar->declType, memberVar->name);

    return result;
}

} // namespace Citron