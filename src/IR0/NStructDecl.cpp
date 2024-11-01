#include "NStructDecl.h"
#include <cassert>
#include <Infra/Exceptions.h>

using namespace std;

namespace Citron {

NDecl* NStructDecl::GetOuter()
{
    return outer.lock()->GetDecl();
}

RIdentifier NStructDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

NDecl* NStructDecl::GetDecl()
{
    return this;
}

RMember NStructDecl::ToRMember(const shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs)
{
    auto sharedStructDecl = dynamic_pointer_cast<NStructDecl>(sharedThis);
    assert(sharedStructDecl);
    return RMember_Struct(typeArgs, sharedStructDecl);
}

optional<RMember> NStructDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    vector<RMember> candidates;

    // type
    if (auto oType = NTypeDeclContainerComponent::GetMemberType(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(*oType);

    // struct member func
    if (auto oFunc = NFuncDeclContainerComponent<NStructMemberFuncDecl>::GetMemberFunc(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(*oFunc);

    if (candidates.empty()) return nullopt;

    if (1 < candidates.size())
    {
        // TODO: 여러 candidate가 있다고 로깅하고 FatalException던지기
        throw NotImplementedException();
    }

    return candidates[1];
}

} // namespace Citron