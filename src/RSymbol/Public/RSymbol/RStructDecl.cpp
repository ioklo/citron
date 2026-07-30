#include "RStructDecl.h"
#include "Infra/Exceptions.h"
#include "RFactory.h"
#include "RStructCtorDecl.h"
#include "RStructVarDecl.h"

using namespace std;

namespace Citron {

RStructDecl::RStructDecl(RDeclKey&& key, RTypeDeclOuter outer, RName&& name, TakeRef<RFactoryPtr> rFactory)
    : key{std::move(key)}
    , outer{outer}
    , name{std::move(name)}
    , trivialCtorIndex{-1}
    , rFactory{rFactory.Take()}
{
}

void RStructDecl::InitTraits(vector<RAppliedDecl<RTraitDecl>>&& traits)
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

RType* RStructDecl::GetOpenType()
{
    return rFactory->MakeStructType(this, MakeOpenTypeArgs(*rFactory));
}

// from RDecl
RDeclKey& RStructDecl::GetDeclKey()
{
    return key;
}

RDecl* RStructDecl::GetOuter() { return outer.GetDecl(); }
RName* RStructDecl::TryGetName()
{
    return &name;
}
size_t RStructDecl::GetTypeParamCount() { return genericsComp.GetTypeParamCount(); }
RTypeParam* RStructDecl::GetTypeParam(size_t index) { return genericsComp.GetTypeParam(index); }

RTypeParam* RStructDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* RStructDecl::GetTypeMember(InRef<RName> name)
{
    return typeDeclContainerComp.GetTypeMember(name);
}

std::optional<RMember> RStructDecl::GetMember(InRef<RName> name)
{
    // 1. type
    if (auto* typeDecl = typeDeclContainerComp.GetTypeMember(name))
        return ToRMember(typeDecl);

    // 2. func
    if (auto o_func = funcDeclContainerComp.GetFuncs(name))
        return move(*o_func);

    // 3. var
    if (auto* var = GetUnboundVar(name))
        return RMember_StructVar{var};

    return nullopt;
}

// from RTypeDecl 
RDecl* RStructDecl::RTypeDecl_GetDecl() { return this; }

void RStructDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron