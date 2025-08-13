#include "NClassDecl.h"

#include <cassert>
#include "Infra/Exceptions.h"

#include "NClassFuncDecl.h"

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
    return RIdentifier { name, typeParams.size(), {} };
}

RMember NClassDecl::ToRMember(RTypeArguments* typeArgs)
{   
    return RMember_Class(typeArgs, this);
}

optional<RMember> NClassDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    vector<RMember> candidates;

    // type
    if (auto oType = NTypeDeclContainerComponent::GetMemberType(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(*oType);

    // class member func
    if (auto oFunc = NFuncDeclContainerComponent<NClassFuncDecl>::GetMemberFunc(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(*oFunc);

    if (explicitTypeParamsExceptOuterCount == 0)
        if (auto oVar = GetVar(typeArgs, name))
            candidates.push_back(*oVar);

    if (candidates.empty()) return nullopt;

    if (1 < candidates.size())
    {
        // TODO: 여러 candidate가 있다고 로깅하고 FatalException던지기
        throw NotImplementedException();
    }

    return candidates[1];
}

optional<RMember> NClassDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, IR0Factory& factory)
{
    auto typeArgs = MakeOpenTypeArgs(factory);

    auto oMember = GetMember(typeArgs, name, explicitTypeParamsExceptOuterCount);
    if (oMember) return oMember;

    return outer->GetNDecl()->GetRDecl()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

optional<RMember_ClassVar> NClassDecl::GetVar(RTypeArguments* typeArgs, const RName& name)
{
    auto i = varsMap.find(name);
    if (i == varsMap.end()) return nullopt;

    return RMember_ClassVar(i->second, typeArgs);
}

} // namespace Citron