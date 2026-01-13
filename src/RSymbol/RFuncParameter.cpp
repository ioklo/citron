#include "RFuncParameter.h"

#include "RTypes.h"

namespace Citron {

RFuncParameter RFuncParameter::Apply(RTypeArguments& typeArgs)
{
    auto appliedType = type->Apply(typeArgs);
    return RFuncParameter{kind, appliedType, name};
}

} // namespace Citron;