#pragma once
#include "IR0Config.h"

#include <optional>
#include <memory>
#include <vector>

#include "RFuncReturn.h"
#include "RFuncParameter.h"
#include "RNames.h"
#include "RMember.h"

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

    std::vector<std::string> typeParams;
    bool bSeqFunc;

    std::optional<FuncReturnAndParams> funcReturnAndParams; // need initialization
    std::optional<std::vector<NStmtPtr>> body;

    std::vector<NLambdaDecl> lambdaDecls;

public:
    IR0_API NCommonFuncDeclComponent(std::vector<std::string>&& typeParams, bool bSeqFunc);
    IR0_API void InitFuncReturnAndParams(RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    IR0_API void InitBody(std::vector<NStmtPtr>&& body);

    IR0_API ~NCommonFuncDeclComponent();

    // internal?
    size_t GetTypeParamCount();
    bool IsSeqFunc() { return bSeqFunc; }

    IR0_API RFuncReturn GetOpenFuncReturn();
    IR0_API RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory);
    IR0_API RFuncParameter& GetOpenFuncParam(int i);
    IR0_API std::vector<RTypePtr> GetParamIds();
    IR0_API std::optional<RMember> ResolveIdentifier(size_t baseTypeParamCount, const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory);
};

}