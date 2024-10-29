#pragma once
#include "IR0Config.h"

#include <optional>
#include <memory>
#include <vector>

#include "RFuncReturn.h"
#include "RFuncParameter.h"

namespace Citron
{
class NLambdaDecl;
using NStmtPtr = std::shared_ptr<class NStmt>;

class RTypeArguments;
class RTypeFactory;

class NCommonFuncDeclComponent
{
    // lambda의 경우, funcReturn이 NeedInduction으로 주어진다.
    struct FuncReturnAndParams
    {
        RFuncReturn funcReturn; // constructor는 RNoneFuncReturn을 씁니다
        std::vector<RFuncParameter> funcParameters;
        bool bLastParameterVariadic;
    };

    std::optional<FuncReturnAndParams> funcReturnAndParams; // need initialization
    std::optional<std::vector<NStmtPtr>> body;

    std::vector<NLambdaDecl> lambdaDecls;

public:
    IR0_API NCommonFuncDeclComponent();
    IR0_API void InitFuncReturnAndParams(RFuncReturn funcReturn, std::vector<RFuncParameter> funcParameters, bool bLastParameterVariadic);
    IR0_API void InitBody(std::vector<NStmtPtr> body);

    IR0_API ~NCommonFuncDeclComponent();

    IR0_API RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory);
    IR0_API std::vector<RTypePtr> GetParamIds();
};

}