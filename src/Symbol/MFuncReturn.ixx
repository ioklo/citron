export module Citron.Symbols:MFuncReturn;

import "std.h";

import :MType;

namespace Citron {

export struct MNoneFuncReturn {}; // for constructor
export struct MConfirmedFuncReturn
{
    MType type;
};
export struct MNeedInferenceFuncReturn {};
export using MFuncReturn = std::variant<MNoneFuncReturn, MConfirmedFuncReturn, MNeedInferenceFuncReturn>;

}

