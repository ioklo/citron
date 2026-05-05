#include "NStructDecl.h"

#include <cassert>

#include "Infra/Exceptions.h"
#include "RSymbol/RFactory.h"
#include "NTypeParamDecl.h"

using namespace std;

namespace Citron {

NStructDecl::NStructDecl(NTypeDeclOuter* outer, RAccessor accessor, RName&& name, const RFactoryPtr& rFactory)
    : outer{outer}, accessor{accessor}, name{move(name)}, rFactory{rFactory}
    , dtor{nullptr}
    , trivialCtorIndex{-1}
{
}

void NStructDecl::InitBaseTypes(RType_Struct* baseStruct, vector<RType*>&& interfaces)
{
    o_baseTypes.emplace(baseStruct, move(interfaces));
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

NDecl* NStructDecl::GetNOuter()
{
    return outer->GetNDecl();
}

RDeclRes NStructDecl::ToRDeclRes(RTypeArguments* typeArgs)
{
    return RDeclRes_Struct(typeArgs, this);
}

RDecl* NStructDecl::GetROuter()
{
    return outer->GetNDecl()->GetRDecl();
}

RIdentifier NStructDecl::GetIdentifier()
{
    return RIdentifier{name, NGenericsComponent::GetTypeParamCount(), {}};
}

RTypeDecl* NStructDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    if (auto* typeDecl = NGenericsComponent::GetTypeMember(name, typeParamCount))
        return typeDecl;
    
    return NTypeDeclContainerComponent::GetTypeMember(name, typeParamCount);
}

optional<RDeclRes> NStructDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    vector<RDeclRes> candidates;

    // type
    if (auto o_type = NTypeDeclContainerComponent::GetMemberType(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(move(*o_type));

    // struct member func
    if (auto o_func = NFuncDeclContainerComponent<NStructFuncDecl>::GetMemberFunc(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(move(*o_func));

    if (explicitTypeParamsExceptOuterCount == 0)
        if (auto o_var = GetVar(typeArgs, name))
            candidates.push_back(move(*o_var));

    if (candidates.empty()) return nullopt;

    if (1 < candidates.size())
    {
        // TODO: 여러 candidate가 있다고 로깅하고 FatalException던지기
        throw NotImplementedException();
    }

    return move(candidates[0]);
}

optional<RDeclRes> NStructDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = NGenericsComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    auto typeArgs = MakeOpenTypeArgs(*rFactory);
    if (auto o_member = GetMember(typeArgs, name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return outer->GetNDecl()->GetRDecl()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

RType* NStructDecl::GetOpenType()
{
    return rFactory->MakeStructType(this, MakeOpenTypeArgs(*rFactory));
}

RType_Struct* NStructDecl::GetUnboundBaseStruct()
{
    assert(o_baseTypes);
    return o_baseTypes->baseStruct;
}

View<RStructVarDecl*> NStructDecl::GetRVars()
{   
    return View<RStructVarDecl*>(&vars, vars.size(),
        [](void* context, size_t i) noexcept -> RStructVarDecl*
        {
            auto* vars = static_cast<std::vector<NStructVarDecl*>*>(context);
            return (*vars)[i];
        });
}

optional<RDeclRes_StructVar> NStructDecl::GetVar(RTypeArguments* typeArgs, const RName& name)
{
    auto i = varsMap.find(name);
    if (i == varsMap.end()) return nullopt;

    return RDeclRes_StructVar(i->second, typeArgs);
}

vector<RStructCtorDecl*> NStructDecl::GetUnboundCtors()
{
    vector<RStructCtorDecl*> result;
    result.reserve(ctors.size());

    for(auto& ctor : ctors)
        result.push_back(ctor);

    return result;
}

RStructCtorDecl* NStructDecl::GetUnboundCopyCtor()
{
    for (auto& ctor : ctors)
    {
        if (ctor->kind == RStructCtorKind::Copy)
            return ctor;
    }

    return nullptr;
}

} // namespace Citron