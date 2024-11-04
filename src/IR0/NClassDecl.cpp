#include "NClassDecl.h"
#include <cassert>
#include <Infra/Exceptions.h>

#include "DeclWithOuterTypeArgs.h"
#include "NClassMemberFuncDecl.h"

using namespace std;

namespace Citron {

RDecl* NClassDecl::GetROuter()
{
    return outer.lock()->GetNDecl()->GetRDecl();
}

RIdentifier NClassDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

RMember NClassDecl::ToRMember(const shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs)
{
    auto sharedClassDecl = dynamic_pointer_cast<NClassDecl>(sharedThis);
    assert(sharedClassDecl);
    return RMember_Class(typeArgs, sharedClassDecl);
}

optional<RMember> NClassDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    vector<RMember> candidates;

    // type
    if (auto oType = NTypeDeclContainerComponent::GetMemberType(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(*oType);

    // class member func
    if (auto oFunc = NFuncDeclContainerComponent<NClassMemberFuncDecl>::GetMemberFunc(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(*oFunc);

    if (explicitTypeParamsExceptOuterCount == 0)
        if (auto oVar = GetMemberVar(typeArgs, name))
            candidates.push_back(*oVar);

    if (candidates.empty()) return nullopt;

    if (1 < candidates.size())
    {
        // TODO: 여러 candidate가 있다고 로깅하고 FatalException던지기
        throw NotImplementedException();
    }

    return candidates[1];
}

optional<RMember_ClassMemberVar> NClassDecl::GetMemberVar(const RTypeArgumentsPtr& typeArgs, const RName& name)
{
    auto i = memberVarsMap.find(name);
    if (i == memberVarsMap.end()) return nullopt;

    return RMember_ClassMemberVar(i->second, typeArgs);
}

} // namespace Citron