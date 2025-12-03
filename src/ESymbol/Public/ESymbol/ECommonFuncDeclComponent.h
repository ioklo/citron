#pragma once

#include <optional>
#include <vector>

#include "EFuncReturn.h"
#include "EFuncParameter.h"

namespace Citron
{

class ECommonFuncDeclComponent
{
    // lambda의 경우, funcReturn이 NeedInduction으로 주어진다.
    struct FuncReturnAndParams
    {
        EFuncReturn funcReturn;
        std::vector<EFuncParameter> funcParameters;
        bool bLastParameterVariadic;
    };

    std::optional<FuncReturnAndParams> funcReturnAndParams; // need initialization
};

}