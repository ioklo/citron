export module Citron.MSymbol:MFuncParameter;

import <string>;
import <memory>;

namespace Citron {

using MTypePtr = std::shared_ptr<class MType>;

export class MFuncParameter
{
    bool bOut;
    MTypePtr type;
    std::string name;
};


}
