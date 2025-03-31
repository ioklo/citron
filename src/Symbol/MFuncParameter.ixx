export module Citron.MDecls:MFuncParameter;

import <string>;
import <memory>;


namespace Citron {

export class MType;
export using MTypePtr = std::shared_ptr<MType>;

export class MFuncParameter
{
    bool bOut;
    MTypePtr type;
    std::string name;
};


}
