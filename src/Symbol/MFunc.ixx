export module Citron.MDecls:MFunc;

import <memory>;
import :MTypeArguments;

namespace Citron {

export class MDeclId;
export using MDeclIdPtr = std::shared_ptr<MDeclId>;

export class MTypeArguments;
export using MTypeArgumentsPtr = std::shared_ptr<MTypeArguments>;

// MFunc는 MDeclIdPtr을 갖고 있다
export class MFunc
{
    MDeclIdPtr declId;
    MTypeArgumentsPtr typeArgs;
};

export using MFuncPtr = std::shared_ptr<MFunc>;


} // namespace Citron