#include "NFactory.h"

#include "RSymbol/RFactory.h"
#include "RSymbol/RNamespaceDeclGroup.h"

#include <cassert>
#include <algorithm>

using namespace std;

namespace Citron {

NFactory::NFactory(RFactoryPtr& rFactory)
    : rFactory{rFactory}
{
}

NFactory::~NFactory() = default;

} // namespace Citron