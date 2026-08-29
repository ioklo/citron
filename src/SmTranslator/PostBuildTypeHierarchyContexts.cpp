#include "PostBuildTypeHierarchyContexts.h"
#include "RSymbol/RAppliedDecl.h"
#include "RSymbol/RFactory.h"
#include "SmTypeTranslation.h"
#include "SmTypeTranslationContexts.h"


using namespace std;

namespace Citron {

expected<RAppliedDecl<RTraitDecl>, DiagPtr> PostBuildTypeHierarchyContexts::MakeTrait(STypeExp* sTypeExp, SmDeclContext* declContext, std::span<RTypeParam*> typeParams)
{
    SmTypeTranslationContexts contexts{SmTypeResolveScope_DeclHeader{declContext, typeParams}, rFactory};
    return TranslateSTypeExpToRTrait(sTypeExp, contexts);
}

} // namespace Citron