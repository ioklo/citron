#include "PostBuildNonTypeSymbolContext.h"
#include "RSymbol/RDecl.h"

namespace Citron {

// struct S<T> { ... }
// impl S : Trait<T> 
PostBuildNonTypeSymbolContext::MakeTraitResult PostBuildNonTypeSymbolContext::MakeTrait(RTypeDecl* rTypeDecl, STypeExp* sTypeExp)
{
    // Trait<T>은 S의 TypeParameter부터 검색을 한다. S의 TypeParameter이외의 자식들은 검색대상이 아니다
    // Trait도 그렇게 찾고, T도 그렇게 찾는다


    return MakeTraitResult{};
}

} // namespace Citron