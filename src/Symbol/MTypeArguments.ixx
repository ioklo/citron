export module Citron.MSymbol:MTypeArguments;

import <vector>;
import <memory>;

import :ForwardDecls;

namespace Citron {

export class MTypeArguments
{
    std::vector<MTypePtr> typeArgs;
};

export using MTypeArgumentsPtr = std::shared_ptr<MTypeArguments>;

// flyweight
export class MTypeArgumentsFactory
{
};


} // namespace Citron