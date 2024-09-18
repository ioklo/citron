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
        MFuncReturn funcReturn;
        std::vector<MFuncParameter> funcParameters;
        bool bLastParameterVariadic;
    };

    std::optional<FuncReturnAndParams> funcReturnAndParams; // need initialization
};

}