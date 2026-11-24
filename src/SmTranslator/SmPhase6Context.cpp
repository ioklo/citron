#include "SmPhase6Context.h"

namespace Citron {

SmPhase6Context::SmPhase6Context(TakeRef<RFactoryPtr> rFactory)
    : rFactory{rFactory.Take()}
{
}

} // namespace Citron