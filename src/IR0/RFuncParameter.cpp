#include "RFuncParameter.h"

#include "RTypes.h"

namespace Citron {

RFuncParameter RFuncParameter::Apply(RTypeArguments& typeArgs, IR0Factory& typeFactory)
{
    auto appliedType = type->Apply(typeArgs, typeFactory);
    return RFuncParameter{bOut, appliedType, name};
}

} // namespace Citron;