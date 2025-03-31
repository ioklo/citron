export module Citron.MDecls:MTypeArguments;

import <vector>;
import <memory>;

namespace Citron {

export class MType;
export using MTypePtr = std::shared_ptr<MType>;

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