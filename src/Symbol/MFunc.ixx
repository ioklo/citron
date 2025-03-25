export module Citron.MSymbol:MFunc;

import :MDeclId;
import :MTypeArguments;

namespace Citron {

// MFunc는 MDeclIdPtr을 갖고 있다

export class MFunc
{
    MDeclIdPtr declId;
    MTypeArgumentsPtr typeArgs;
};

export using MFuncPtr = std::shared_ptr<MFunc>;


} // namespace Citron