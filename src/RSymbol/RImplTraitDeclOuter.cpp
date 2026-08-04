#include "RImplTraitDeclOuter.h"
#include "RNamespace.h"
#include "RClassDecl.h"
#include "RStructDecl.h"

using namespace std;

namespace Citron {

void RImplTraitDeclOuter::AddImplTrait(RImplTraitDecl* decl)
{
    visit([decl](auto* outer) {
        using T = remove_cvref_t<decltype(outer)>;

        if constexpr (same_as<T, RNamespace*>)
        {
            outer->AddImplTrait(decl);
        }
        else if constexpr (same_as<T, RClassDecl*>)
        {
            outer->AddImplTrait(decl);
        }
        else if constexpr (same_as<T, RStructDecl*>)
        {
            outer->AddImplTrait(decl);
        }
        else static_assert(false);
    }, v);
}

RDecl* RImplTraitDeclOuter::GetDecl()
{
    return visit([](auto* outer) -> RDecl* { return outer; }, v);
}

} // namespace Citron