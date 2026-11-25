#include "RTypeParam.h"
#include <cassert>
#include "RFactory.h"
#include "RTypeArguments.h"

using namespace std;

namespace Citron {

RTypeParam::RTypeParam(RDecl* owner, RName&& name, size_t globalIndex, InRefPtr<RFactory> rFactory)
    : owner{owner}
    , name{move(name)}
    , globalIndex{globalIndex}
    , rFactory{rFactory.Share()}
{
}

} // namespace Citron