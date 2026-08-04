#include "PostBuildNonTypeSymbolContext.h"
#include "RSymbol/RDecl.h"
#include "SmDeclContext.h"
#include "SmTypeTranslation.h"
#include "SmTypeTranslationContexts.h"

using namespace std;

namespace Citron {

// struct S<T> { ... }
// impl S : Trait<T> 
expected<RAppliedDecl<RTraitDecl>, DiagPtr> PostBuildNonTypeSymbolContext::MakeTrait(STypeExp* sTypeExp, SmDeclContext* declContext, std::span<RTypeParam*> typeParams)
{
    SmTypeTranslationContexts contexts{SmTypeResolveScope_DeclHeader{declContext, typeParams}, rFactory};

    return TranslateSTypeExpToRTrait(sTypeExp, contexts);
}

} // namespace Citron