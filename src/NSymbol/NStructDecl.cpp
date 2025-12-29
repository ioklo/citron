#include "NStructDecl.h"

#include <cassert>

#include "Infra/Exceptions.h"

using namespace std;

namespace Citron {

NStructDecl::NStructDecl(NTypeDeclOuter* outer, RAccessor accessor, RName&& name, vector<string>&& typeParams)
    : outer{outer}, accessor{accessor}, name{move(name)}, typeParams(move(typeParams)), dtor{nullptr}
{
}

void NStructDecl::InitBaseTypes(RType_Struct* baseStruct, vector<RType_Interface*>&& interfaces)
{
    oBaseTypes = BaseTypes{baseStruct, move(interfaces)};
}

void NStructDecl::AddCtor(NStructCtorDecl* decl)
{
    ctors.push_back(decl);
}

void NStructDecl::AddDtor(NStructDtorDecl* decl)
{
    assert(!dtor);
    dtor = decl;
}

void NStructDecl::AddVar(NStructVarDecl* decl)
{
    vars.push_back(decl);
    varsMap.try_emplace(RName_Normal{decl->name}, decl);
}

NStructCtorDecl* NStructDecl::GetUnboundTrivialCtor_NStructCtorDecl()
{
    if (trivialCtorIndex == -1) return nullptr;
    return ctors[trivialCtorIndex];
}

RType_Struct* NStructDecl::GetUnboundBaseStruct()
{
    assert(oBaseTypes);
    return oBaseTypes->baseStruct;
}

NDecl* NStructDecl::GetNOuter()
{
    return outer->GetNDecl();
}

RMember NStructDecl::ToRMember(RTypeArguments* typeArgs)
{
    return RMember_Struct(typeArgs, this);
}

RDecl* NStructDecl::GetROuter()
{
    return outer->GetNDecl()->GetRDecl();
}

RIdentifier NStructDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), {} };
}

optional<RMember> NStructDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
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

    return candidates[0];
}

optional<RMember> NStructDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory)
{
    auto typeArgs = MakeOpenTypeArgs(factory);
    if (auto oMember = GetMember(typeArgs, name, explicitTypeParamsExceptOuterCount))
        return oMember;

    return outer->GetNDecl()->GetRDecl()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

optional<RMember_StructVar> NStructDecl::GetVar(RTypeArguments* typeArgs, const RName& name)
{
    auto i = varsMap.find(name);
    if (i == varsMap.end()) return nullopt;

    return RMember_StructVar(i->second, typeArgs);
}

vector<RStructCtorDecl*> NStructDecl::GetUnboundCtors()
{
    vector<RStructCtorDecl*> result;
    result.reserve(ctors.size());

    for(auto& ctor : ctors)
        result.push_back(ctor);

    return result;
}

} // namespace Citron