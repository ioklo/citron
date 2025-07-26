module;
#include <variant>
#include <memory>

export module Citron.MDecls:MFuncReturn;

namespace Citron {

export class MType;
export using MTypePtr = std::shared_ptr<MType>;

export struct MFuncReturn_ForCtor {}; // for ctor
export struct MFuncReturn_Normal
{
    MTypePtr type;
};

export using MFuncReturn = std::variant<MFuncReturn_ForCtor, MFuncReturn_Normal>;

}

