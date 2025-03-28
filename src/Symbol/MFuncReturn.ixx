export module Citron.MSymbol:MFuncReturn;

import :ForwardDecls;
import <variant>;
import <memory>;

namespace Citron {

export struct MFuncReturn_ForCtor {}; // for ctor
export struct MFuncReturn_Normal
{
    MTypePtr type;
};

export using MFuncReturn = std::variant<MFuncReturn_ForCtor, MFuncReturn_Normal>;

}

