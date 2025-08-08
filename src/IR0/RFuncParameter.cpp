#include "RFuncParameter.h"

#include "RTypes.h"

namespace Citron {

RFuncParameter RFuncParameter::Apply(RTypeArguments& typeArgs, RTypeFactory& typeFactory)
{
    auto appliedType = type->Apply(typeArgs, typeFactory);
    return RFuncParameter{bOut, appliedType, name};
}

} // namespace Citron;