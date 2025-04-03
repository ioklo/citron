export module Citron.RDecls:RFuncParameter;

import <string>;
import <memory>;

import :RNames;

namespace Citron {

export class RType;
export using RTypePtr = std::shared_ptr<RType>;

export class RTypeArguments;
export class RTypeFactory;

export struct RFuncParameter
{
    bool bOut;
    RTypePtr type; // 람다의 경우 지정이 안될 수 있다
    RName name;

    RFuncParameter Apply(RTypeArguments& typeArgs, RTypeFactory& typeFactory);
};


}
