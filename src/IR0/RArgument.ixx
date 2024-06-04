export module Citron.RArgument;

import <variant>;
import Citron.RExp;

namespace Citron {

export class RNormalArgument
{
    RExp exp;
};

export class RParamsArgument
{
    RExp exp;
    int elemCount;
};

export using RArgument = std::variant<RNormalArgument, RParamsArgument>;

}