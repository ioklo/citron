#include "SynthesizeImplicitSymbolContext.h"

namespace Citron {

SynthesizeImplicitSymbolContext::SynthesizeImplicitSymbolContext(TakeRef<RFactoryPtr> rFactory)
    : rFactory{rFactory.Take()}
{
}

} // namespace Citron