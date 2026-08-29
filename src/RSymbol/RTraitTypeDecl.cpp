#include "RTraitTypeDecl.h"
#include "RTraitDecl.h"
#include "RMember.h"

using namespace std;

namespace Citron {
RTraitTypeDecl::RTraitTypeDecl(RTraitDecl* traitDecl, RDeclKey&& key, RName&& name)
    : traitDecl{traitDecl}, key{std::move(key)}, name{std::move(name)}
{
}

void RTraitTypeDecl::Init(std::vector<RAppliedDecl<RTraitDecl>>&& traits)
{
    this->traits = move(traits);
}

RDeclKey& RTraitTypeDecl::GetDeclKey()
{
    return key;
}

RDecl* RTraitTypeDecl::GetOuter()
{
    return traitDecl;
}

RName* RTraitTypeDecl::TryGetName()
{
    return &name;
}

size_t RTraitTypeDecl::GetTypeParamCount()
{
    // TODO: [78] 2026-08-29, trait type requirements에 type parameter가 들어올 수 있게
    return 0;
}

RTypeParam* RTraitTypeDecl::GetTypeParam(size_t index)
{
    // TODO: [78] 2026-08-29, trait type requirements에 type parameter가 들어올 수 있게
    return nullptr;
}

RTypeParam* RTraitTypeDecl::GetTypeParam(InRef<RName> name)
{
    // TODO: [78] 2026-08-29, trait type requirements에 type parameter가 들어올 수 있게
    return nullptr;
}

RTypeDecl* RTraitTypeDecl::GetTypeMember(InRef<RName> name)
{
    // TODO: [78] 2026-08-29, trait type requirements에 type parameter가 들어올 수 있게
    return nullptr;
}

optional<RMember> RTraitTypeDecl::GetMember(InRef<RName> name)
{
    // TODO: [78] 2026-08-29, trait type requirements에 type parameter가 들어올 수 있게
    return nullopt;
}

} // namespace Citron