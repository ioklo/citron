module;
#include <string>
#include <memory>
export module Citron.MDecls:MFuncParameter;

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
