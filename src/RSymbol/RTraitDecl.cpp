#include "RTraitDecl.h"
#include <optional>
#include "RFactory.h"

using namespace std;

namespace Citron {
RTraitDecl::RTraitDecl(RTypeDeclOuter outer, RName&& name, TakeRef<RFactoryPtr> rFactory)
    : outer{std::move(outer)}, name{std::move(name)}
    , genericsComp{}
    , rFactory{rFactory.Take()}
{
}

void RTraitDecl::InitTypeParams(vector<RTypeParamDecl*>&& typeParams)
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

RTypeParamDecl* RTraitDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeDecl* RTraitDecl::GetTypeMember(InRef<RName> name)
{
    return genericsComp.GetTypeMember(name);
}

optional<RDeclRes> RTraitDecl::ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    // 지금은 func group만 모은다
    vector<TDeclWithOuterTypeArgs<RTraitFuncDecl>> funcs;

    // TODO: [69] 2026-07-12, GetMember에서 Generics도 제대로 리턴하도록

    // member들 중에
    for (auto& member : members)
    {
        auto* decl = member.GetDecl();
        auto id = decl->GetIdentifier();

        if (id.name != *name) continue;
        if (decl->GetTypeParamCount() < explicitTypeParamsExceptOuterCount) continue;

        member.Visit([&funcs, typeArgs](auto* memberDecl) {
            using T = std::remove_cvref_t<decltype(memberDecl)>;

            if constexpr (std::same_as<T, RTraitFuncDecl*>) funcs.emplace_back(memberDecl, typeArgs);
            else static_assert(false);
        });
    }

    if (!funcs.empty()) return RDeclRes_TraitFuncs{move(funcs)};

    return nullopt;
}

std::optional<RDeclRes> RTraitDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    // TODO: [69] 2026-07-12, GetMember에서 Generics도 제대로 리턴하도록
    if (auto o_member = genericsComp.ResolveTypeParam(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    auto typeArgs = MakeOpenTypeArgs(*rFactory);
    if (auto o_member = ResolveMember(typeArgs, name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return outer.GetDecl()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

} // namespace Citron