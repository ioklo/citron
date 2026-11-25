#include "SmPhase6Context.h"

namespace Citron {

SmPhase6Context::SmPhase6Context(InRefPtr<RFactory> rFactory)
    : rFactory{rFactory.Share()}
{
}

} // namespace Citron