#pragma once
#include "IR0Config.h"

#include <optional>
#include <memory>
#include <vector>

#include "RFuncReturn.h"
#include "RFuncParameter.h"

namespace Citron
{
class RLambdaDecl;
using RStmtPtr = std::shared_ptr<class RStmt>;

class RTypeArguments;
class RTypeFactory;

class RCommonFuncDeclComponent
{
    // lambda의 경우, funcReturn이 NeedInduction으로 주어진다.
    struct FuncReturnAndParams
    {
        RFuncReturn funcReturn; // constructor는 RNoneFuncReturn을 씁니다
        std::vector<RFuncParameter> funcParameters;
        bool bLastParameterVariadic;
    };

    std::optional<FuncReturnAndParams> funcReturnAndParams; // need initialization
    std::optional<std::vector<RStmtPtr>> body;

    std::vector<RLambdaDecl> lambdaDecls;

public:
    IR0_API RCommonFuncDeclComponent();
    IR0_API void InitFuncReturnAndParams(RFuncReturn funcReturn, std::vector<RFuncParameter> funcParameters, bool bLastParameterVariadic);
    IR0_API void InitBody(std::vector<RStmtPtr> body);

    IR0_API ~RCommonFuncDeclComponent();

    IR0_API RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory);
    IR0_API std::vector<RTypePtr> GetParamIds();
};

}