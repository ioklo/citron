module Citron.RDecls:RFuncParameter;

import :RTypes;

namespace Citron {

RFuncParameter RFuncParameter::Apply(RTypeArguments& typeArgs, RTypeFactory& typeFactory)
{
    auto appliedType = type->Apply(typeArgs, typeFactory);
    return RFuncParameter{bOut, std::move(appliedType), name};
}

} // namespace Citron;