#include "NClassDecl.h"

#include <cassert>
#include <ranges>

#include "Infra/Exceptions.h"
#include "NClassFuncDecl.h"
#include "NTypeParamDecl.h"

using namespace std;

namespace Citron {

NDecl* NClassDecl::GetNOuter()
{
    return outer->GetNDecl();
}

RDecl* NClassDecl::GetROuter()
{
    return outer->GetNDecl()->GetRDecl();
}

RIdentifier NClassDecl::GetIdentifier()
{
    return RIdentifier{name, typeParams.size(), {}};
}

RTypeParamDecl* NClassDecl::GetTypeParam(size_t index)
{
    return typeParams[index];
}

RTypeDecl* NClassDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    // TODO: [26] typeParams에서도 검색 (NTypeParamDecl 추가 필요)
    return NTypeDeclContainerComponent::GetTypeMember(name, typeParamCount);
}

RMember NClassDecl::ToRMember(RTypeArguments* typeArgs)
{   
    return RMember_Class(typeArgs, this);
}

optional<RMember> NClassDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    vector<RMember> candidates;

    // type
    if (auto o_type = NTypeDeclContainerComponent::GetMemberType(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(*o_type);

    // class member func
    if (auto o_func = NFuncDeclContainerComponent<NClassFuncDecl>::GetMemberFunc(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(*o_func);

    if (explicitTypeParamsExceptOuterCount == 0)
        if (auto o_var = GetVar(typeArgs, name))
            candidates.push_back(*o_var);

    if (candidates.empty()) return nullopt;

    if (1 < candidates.size())
    {
        // TODO: 여러 candidate가 있다고 로깅하고 FatalException던지기
        throw NotImplementedException();
    }

    return candidates[1];
}

optional<RMember> NClassDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    auto* typeArgs = MakeOpenTypeArgs(*rFactory);

    auto o_member = GetMember(typeArgs, name, explicitTypeParamsExceptOuterCount);
    if (o_member) return o_member;

    return outer->GetNDecl()->GetRDecl()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

optional<RMember_ClassVar> NClassDecl::GetVar(RTypeArguments* typeArgs, const RName& name)
{
    auto i = varsMap.find(name);
    if (i == varsMap.end()) return nullopt;

    return RMember_ClassVar(i->second, typeArgs);
}

} // namespace Citron