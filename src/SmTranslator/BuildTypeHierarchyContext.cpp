#include "BuildTypeHierarchyContext.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "SmTypeTranslation.h"
#include "SmTypeResolveScope.h"
#include "SmTypeTranslationContexts.h"

using namespace std;

namespace Citron {

expected<RAppliedDecl<RTraitDecl>, DiagPtr> BuildTypeHierarchyContext::MakeTrait(STypeExp* sType, SmTypeResolveScope scope)
{
    SmTypeTranslationContexts contexts{scope, rFactory};
    return TranslateSTypeExpToRTrait(sType, contexts);
}

} // namespace Citron