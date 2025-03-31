export module Citron.RDecls:RFuncReturn;

import <memory>;
import <variant>;

namespace Citron {

export class RType;
export using RTypePtr = std::shared_ptr<RType>;

export struct RFuncReturn_ForCtor {};
export struct RFuncReturn_Set
{
    RTypePtr type;
};
export struct RFuncReturn_NotSet {}; // need inference
export using RFuncReturn = std::variant<RFuncReturn_ForCtor, RFuncReturn_Set, RFuncReturn_NotSet>;

}

