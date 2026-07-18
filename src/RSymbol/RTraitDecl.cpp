#include "RTraitDecl.h"
#include <optional>
#include "RFactory.h"
#include "RMember.h"

using namespace std;

namespace Citron {
RTraitDecl::RTraitDecl(RTypeDeclOuter outer, RName&& name, TakeRef<RFactoryPtr> rFactory)
    : outer{move(outer)}, name{move(name)}
    , genericsComp{}
    , rFactory{rFactory.Take()}
{
}

void RTraitDecl::InitTypeParams(vector<RTypeParam*>&& typeParams)
{
    return genericsComp.InitTypeParams(move(typeParams));
}

RDecl* RTraitDecl::GetOuter()
{
    return outer.GetDecl();
}

RIdentifier RTraitDecl::GetIdentifier()
{
    return RIdentifier{name, {}};
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
        if (member.GetDecl()->GetIdentifier().name == *name)
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

} // namespace Citron