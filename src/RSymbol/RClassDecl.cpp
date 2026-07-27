#include "RClassDecl.h"
#include "Infra/Exceptions.h"
#include "RFactory.h"
#include "RTypeRes.h"
#include "RDeclRes.h"
#include "RTypeArguments.h"

using namespace std;

namespace Citron {

RClassDecl::RClassDecl(RDeclKey&& key, RTypeDeclOuter&& outer, TakeRef<RName> name, TakeRef<RFactoryPtr> rFactory)
    : key{std::move(key)}, outer{std::move(outer)}, name{name.Take()}, rFactory{rFactory.Take()}, trivialCtorIndex{-1}
    , genericsComp{}, typeDeclContainerComp{}
{
}

RClassVarDecl* RClassDecl::GetUnboundVar(InRef<RName> name)
{
    auto i = varsMap.find(*name);
    if (i == varsMap.end()) return nullptr;

    return i->second;
}

optional<RDeclRes_ClassVar> RClassDecl::ResolveVar(RTypeArguments* typeArgs, InRef<RName> name)
{
    auto i = varsMap.find(*name);
    if (i == varsMap.end()) return nullopt;

    return RDeclRes_ClassVar{i->second, typeArgs};
}

// from RDecl

RType* RClassDecl::GetOpenType()
{
    return rFactory->MakeClassType(this, MakeOpenTypeArgs(*rFactory));
}

RDeclKey& RClassDecl::GetDeclKey()
{
    return key;
}

RDecl* RClassDecl::GetOuter()
{
    return outer.GetDecl();
}

RName* RClassDecl::TryGetName()
{
    return &name;
}

size_t RClassDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParam* RClassDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeParam* RClassDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* RClassDecl::GetTypeMember(InRef<RName> name)
{
    return typeDeclContainerComp.GetTypeMember(name);
}

optional<RMember> RClassDecl::GetMember(InRef<RName> name)
{
    // 1. type
    if (auto* typeDecl = typeDeclContainerComp.GetTypeMember(name))
        return ToRMember(typeDecl);

    // 2. func
    if (auto o_func = funcDeclContainerComp.GetFuncs(name))
        return move(*o_func);

    // 3. var
    if (auto* var = GetUnboundVar(name))
        return RMember_ClassVar{var};

    return nullopt;
}

optional<RTypeRes> RClassDecl::ResolveInheritedTypeMember(RTypeArguments* typeArgs, InRef<RName> name)
{
    assert(o_baseTypes);

    // baseClass가 있다면
    if (o_baseTypes->baseClass)
    {
        auto* baseClassTypeArgs = o_baseTypes->baseClass->typeArgs->Apply(typeArgs);

        if (auto* baseTypeMember = o_baseTypes->baseClass->decl->GetTypeMember(name))
            return ToRTypeRes(baseClassTypeArgs, baseTypeMember);

        return o_baseTypes->baseClass->decl->ResolveInheritedTypeMember(baseClassTypeArgs, name);
    }

    return nullopt;
}

optional<RDeclRes> RClassDecl::ResolveInheritedMember(RTypeArguments* typeArgs, InRef<RName> name)
{
    assert(o_baseTypes);

    // baseClass가 있다면
    if (o_baseTypes->baseClass)
    {
        auto* baseClassTypeArgs = o_baseTypes->baseClass->typeArgs->Apply(typeArgs);

        if (auto o_baseMember = o_baseTypes->baseClass->decl->GetMember(name))
            return ToRDeclRes(baseClassTypeArgs, *o_baseMember);

        return o_baseTypes->baseClass->decl->ResolveInheritedMember(baseClassTypeArgs, name);
    }

    return nullopt;
}

RDecl* RClassDecl::RTypeDecl_GetDecl()
{
    return this;
}

void RClassDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron