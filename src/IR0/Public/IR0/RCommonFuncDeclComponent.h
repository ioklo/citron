#pragma once
#include <optional>
#include <memory>
#include <vector>

#include "RFuncReturn.h"
#include "RFuncParameter.h"

namespace Citron
{
class RLambdaDecl;

class RCommonFuncDeclComponent
{
    // lambda의 경우, funcReturn이 NeedInduction으로 주어진다.
    struct FuncReturnAndParams
    {
        std::optional<RFuncReturn> funcReturn;     // nullopt for constructor
        std::vector<RFuncParameter> funcParameters;
    };

    std::optional<FuncReturnAndParams> funcReturnAndParams; // need initialization
    std::optional<bool> bLastParameterVariadic;             // need initialization
    std::vector<RLambdaDecl> lambdaDecls;
};

}