#include "BuildImplicitSymbolContext.h"

namespace Citron {

BuildImplicitSymbolContext::BuildImplicitSymbolContext(TakeRef<RFactoryPtr> rFactory)
    : rFactory{rFactory.Take()}
{
}

} // namespace Citron