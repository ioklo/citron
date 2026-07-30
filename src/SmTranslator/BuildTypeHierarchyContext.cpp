#include "BuildTypeHierarchyContext.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "SmTypeTranslation.h"

using namespace std;

namespace Citron {

expected<RAppliedDecl<RTraitDecl>, DiagPtr> BuildTypeHierarchyContext::MakeTrait(STypeExp* sType, SmTypeResolveScope scope)
{
    return TranslateSTypeExpToRTrait(sType, scope, rFactory);
}

} // namespace Citron