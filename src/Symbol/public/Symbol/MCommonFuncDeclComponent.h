#pragma once
#include <optional>

#include "MFuncReturn.h"
#include "MFuncParameter.h"

namespace Citron
{

class MCommonFuncDeclComponent
{
    // lambda의 경우, funcReturn이 NeedInduction으로 주어진다.
    struct FuncReturnAndParams
    {
        std::optional<MFuncReturn> funcReturn;     // nullopt for constructor
        std::vector<MFuncParameter> funcParameters;
    };

    std::optional<FuncReturnAndParams> funcReturnAndParams; // need initialization
    std::optional<bool> bLastParameterVariadic;             // need initialization
};

}