module;
#include "Macros.h"

export module Citron.Symbols:MCommonFuncDeclComponent;

import <optional>;
import <vector>;
import :MFuncReturn;
import :MFuncParameter;

namespace Citron
{
class MLambdaDecl;

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
    std::vector<MLambdaDecl> lambdaDecls;

};

}