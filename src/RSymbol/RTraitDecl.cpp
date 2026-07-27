#include "RTraitDecl.h"
#include <optional>
#include "RFactory.h"
#include "RMember.h"

using namespace std;

namespace Citron {
RTraitDecl::RTraitDecl(RDeclKey&& key, RTypeDeclOuter outer, RName&& name, TakeRef<RFactoryPtr> rFactory)
    : key{move(key)}, outer{move(outer)}, name{move(name)}
    , rFactory{rFactory.Take()}
    , genericsComp{}
{
}

void RTraitDecl::InitTypeParams(vector<RTypeParam*>&& typeParams)
{
    return genericsComp.InitTypeParams(move(typeParams));
}

void RTraitDecl::AddMember(RTraitMemberDecl&& decl)
{
    members.push_back(std::move(decl));
}

RDeclKey& RTraitDecl::GetDeclKey()
{
    return key;
}

RDecl* RTraitDecl::GetOuter()
{
    return outer.GetDecl();
}

RName* RTraitDecl::TryGetName()
{
    return &name;
}

size_t RTraitDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParam* RTraitDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeParam* RTraitDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* RTraitDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RTraitDecl::GetMember(InRef<RName> name)
{
    for (auto& member : members)
    {
        auto* memberName = member.GetDecl()->TryGetName();
        if (!memberName) continue;

        if (*memberName == *name)
        {
            return member.Visit([](auto* memberDecl) -> RMember {
                using T = remove_cvref_t<decltype(memberDecl)>;

                if constexpr (same_as<T, RTraitFuncDecl*>) return RMember_TraitFuncs{{memberDecl}};
                else static_assert(false);
            });
        }
    }

    return nullopt;
}

RDecl* RTraitDecl::RTypeDecl_GetDecl()
{
    return this;
}

void RTraitDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron