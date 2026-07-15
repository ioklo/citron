#include "RStructDecl.h"
#include "Infra/Exceptions.h"
#include "RFactory.h"
#include "RStructCtorDecl.h"
#include "RStructVarDecl.h"

using namespace std;

namespace Citron {

RStructDecl::RStructDecl(RTypeDeclOuter outer, TakeRef<RName> name, TakeRef<RFactoryPtr> rFactory)
    : outer{outer}
    , name{name.Take()}
    , trivialCtorIndex{-1}
    , rFactory{rFactory.Take()}
{
}

void RStructDecl::InitTraits(std::vector<RType_Trait*>&& traits)
{
    o_traits.emplace(std::move(traits));
}

void RStructDecl::AddCtor(RStructCtorDecl* decl)
{
    ctors.push_back(decl);
}

void RStructDecl::AddDtor(RStructDtorDecl* decl)
{
    assert(!dtor);
    dtor = decl;
}

void RStructDecl::AddVar(RStructVarDecl* decl)
{
    vars.push_back(decl);
    varsMap.try_emplace(decl->GetName(), decl);
}

RStructVarDecl* RStructDecl::GetUnboundVar(InRef<RName> name)
{
    auto i = varsMap.find(*name);
    if (i == varsMap.end()) return nullptr;

    return i->second;
}

RStructCtorDecl* RStructDecl::GetUnboundCopyCtor()
{
    for (auto* ctor : ctors)
    {
        if (ctor->GetKind() == RStructCtorKind::Copy)
            return ctor;
    }

    return nullptr;
}

// from RDecl
RDecl* RStructDecl::GetOuter() { return outer.GetDecl(); }
RIdentifier RStructDecl::GetIdentifier() { return RIdentifier{name, {}}; }
size_t RStructDecl::GetTypeParamCount() { return genericsComp.GetTypeParamCount(); }
RTypeParamDecl* RStructDecl::GetTypeParam(size_t index) { return genericsComp.GetTypeParam(index); }
RTypeDecl* RStructDecl::GetTypeMember(InRef<RName> name)
{
    if (auto* typeDecl = genericsComp.GetTypeMember(name))
        return typeDecl;

    return typeDeclContainerComp.GetTypeMember(name);
}

std::optional<RDeclRes> RStructDecl::ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    vector<RDeclRes> candidates;

    // type
    if (auto o_type = typeDeclContainerComp.ResolveTypeMember(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(move(*o_type));

    // struct member func
    if (auto o_func = funcDeclContainerComp.GetMemberFunc(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(move(*o_func));

    if (explicitTypeParamsExceptOuterCount == 0)
        if (auto* var = GetUnboundVar(name))
            candidates.push_back(RDeclRes_StructVar(var, typeArgs));

    if (candidates.empty()) return nullopt;

    if (1 < candidates.size())
    {
        // TODO: 여러 candidate가 있다고 로깅하고 FatalException던지기
        throw NotImplementedException();
    }

    return move(candidates[0]);
}

std::optional<RDeclRes> RStructDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = genericsComp.ResolveTypeParam(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    auto typeArgs = MakeOpenTypeArgs(*rFactory);
    if (auto o_member = ResolveMember(typeArgs, name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return outer.GetDecl()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

// from RTypeDecl 
RDecl* RStructDecl::RTypeDecl_GetDecl() { return this; }
RType* RStructDecl::GetOpenType()
{
    return rFactory->MakeStructType(this, MakeOpenTypeArgs(*rFactory));
}

RDeclRes RStructDecl::ToRDeclRes(RTypeArguments* typeArgs)
{
    return RDeclRes_Struct(typeArgs, this);
}

void RStructDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron