#include "NStructDecl.h"
#include <cassert>
#include <Infra/Exceptions.h>

#include "DeclWithOuterTypeArgs.h"
#include "RStructMemberFuncDecl.h"

using namespace std;

namespace Citron {

NStructDecl::NStructDecl(NTypeDeclOuterWPtr&& outer, RAccessor accessor, RName&& name, vector<string>&& typeParams)
    : outer(std::move(outer)), accessor(accessor), name(std::move(name)), typeParams(std::move(typeParams))
{
}

shared_ptr<NStructConstructorDecl> NStructDecl::GetOpenTrivialConstructor()
{
    if (trivialConstructorIndex == -1) return nullptr;
    return constructors[trivialConstructorIndex];
}

NDecl* NStructDecl::GetNOuter()
{
    return outer.lock()->GetNDecl();
}

RMember NStructDecl::ToRMember(const shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs)
{
    auto sharedStructDecl = dynamic_pointer_cast<NStructDecl>(sharedThis);
    assert(sharedStructDecl);
    return RMember_Struct(typeArgs, sharedStructDecl);
}

RDecl* NStructDecl::GetROuter()
{
    return outer.lock()->GetNDecl()->GetRDecl();
}

RIdentifier NStructDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
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

optional<RMember> NStructDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    auto sharedOuter = outer.lock();
    assert(sharedOuter);

    auto typeArgs = MakeOpenTypeArgs(factory);
    if (auto oMember = GetMember(typeArgs, name, explicitTypeParamsExceptOuterCount))
        return oMember;

    return sharedOuter->GetNDecl()->GetRDecl()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

optional<RMember_StructMemberVar> NStructDecl::GetMemberVar(const RTypeArgumentsPtr& typeArgs, const RName& name)
{
    auto i = memberVarsMap.find(name);
    if (i == memberVarsMap.end()) return nullopt;

    return RMember_StructMemberVar(i->second, typeArgs);
}

vector<shared_ptr<RStructConstructorDecl>> NStructDecl::GetUnboundConstructors()
{
    vector<shared_ptr<RStructConstructorDecl>> result;
    result.reserve(constructors.size());

    for(auto& constructor : constructors)
        result.push_back(constructor);

    return result;
}

} // namespace Citron