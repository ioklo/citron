#include "RClassDecl.h"
#include "Infra/Exceptions.h"
#include "RFactory.h"
#include "RTypeArguments.h"

using namespace std;

namespace Citron {

RClassDecl::RClassDecl(RDeclKey&& key, RTypeDeclOuter&& outer, TakeRef<RName> name, TakeRef<RFactoryPtr> rFactory)
    : key{std::move(key)}, outer{std::move(outer)}, name{name.Take()}, rFactory{rFactory.Take()}, trivialCtorIndex{-1}
    , genericsComp{}, typeDeclContainerComp{}
{
}

optional<RAppliedDecl<RClassDecl>> RClassDecl::GetUnboundBaseClass()
{
    assert(o_baseTypes);
    return o_baseTypes->o_baseClass;
}

RClassVarDecl* RClassDecl::GetUnboundVar(InRef<RName> name)
{
    auto i = varsMap.find(*name);
    if (i == varsMap.end()) return nullptr;

    return i->second;
}

// from RDecl
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

RDecl* RClassDecl::RTypeDecl_GetDecl()
{
    return this;
}

void RClassDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron