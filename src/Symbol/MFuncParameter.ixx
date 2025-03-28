export module Citron.MSymbol:MFuncParameter;

import <string>;
import <memory>;

import :ForwardDecls;

namespace Citron {

export class MFuncParameter
{
    bool bOut;
    MTypePtr type;
    std::string name;
};


}
