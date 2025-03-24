#include "NStructDecl.h"
#include <cassert>
#include <Infra/Exceptions.h>

#include "DeclWithOuterTypeArgs.h"
#include "RStructFuncDecl.h"

using namespace std;

namespace Citron {

NStructDecl::NStructDecl(NTypeDeclOuterWPtr&& outer, RAccessor accessor, RName&& name, vector<string>&& typeParams)
    : outer(std::move(outer)), accessor(accessor), name(std::move(name)), typeParams(std::move(typeParams))
{
}

void NStructDecl::InitBaseTypes(shared_ptr<RType_Struct>&& baseStruct, vector<shared_ptr<RType_Interface>>&& interfaces)
{
    oBaseTypes = BaseTypes { std::move(baseStruct), std::move(interfaces) };
}

void NStructDecl::AddCtor(std::shared_ptr<NStructCtorDecl>&& decl)
{
    ctors.push_back(std::move(decl));
}

void NStructDecl::AddVar(std::shared_ptr<NStructVarDecl>&& decl)
{
    vars.push_back(std::move(decl));
}

shared_ptr<NStructCtorDecl> NStructDecl::GetUnboundTrivialCtor_NStructCtorDecl()
{
    if (trivialCtorIndex == -1) return nullptr;
    return ctors[trivialCtorIndex];
}

shared_ptr<RType_Struct> NStructDecl::GetUnboundBaseStruct()
{
    assert(oBaseTypes);
    return oBaseTypes->baseStruct;
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
    if (auto oFunc = NFuncDeclContainerComponent<NStructFuncDecl>::GetMemberFunc(typeArgs, name, explicitTypeParamsExceptOuterCount))
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

optional<RMember> NStructDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    auto sharedOuter = outer.lock();
    assert(sharedOuter);

    auto typeArgs = MakeOpenTypeArgs(factory);
    if (auto oMember = GetMember(typeArgs, name, explicitTypeParamsExceptOuterCount))
        return oMember;

    return sharedOuter->GetNDecl()->GetRDecl()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

optional<RMember_StructVar> NStructDecl::GetVar(const RTypeArgumentsPtr& typeArgs, const RName& name)
{
    auto i = varsMap.find(name);
    if (i == varsMap.end()) return nullopt;

    return RMember_StructVar(i->second, typeArgs);
}

vector<shared_ptr<RStructCtorDecl>> NStructDecl::GetUnboundCtors()
{
    vector<shared_ptr<RStructCtorDecl>> result;
    result.reserve(ctors.size());

    for(auto& ctor : ctors)
        result.push_back(ctor);

    return result;
}

} // namespace Citron