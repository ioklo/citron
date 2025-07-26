module;
#include <optional>
#include <vector>

export module Citron.MDecls:MCommonFuncDeclComponent;

import :MFuncReturn;
import :MFuncParameter;

namespace Citron
{

export class MCommonFuncDeclComponent
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