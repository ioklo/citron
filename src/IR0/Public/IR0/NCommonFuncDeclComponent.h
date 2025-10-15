#pragma once
#include "IR0Config.h"

#include <optional>
#include <vector>
#include <string>

#include "RFuncReturn.h"
#include "RMember.h"
#include "RNames.h"

namespace Citron {

struct RFuncParameter;
class RFactory;
class RTypeArguments;

class NStmt;

class NLambdaVarDecl;
class NLambdaDecl;

class NCommonFuncDeclComponent
{
    // lambda의 경우, funcReturn이 NeedInduction으로 주어진다.
    struct FuncReturnAndParams
    {
        RFuncReturn funcReturn; // ctor는 RReturn_ForCtor를 씁니다
        std::vector<RFuncParameter> funcParameters;
        bool bLastParameterVariadic;
    };

    struct Body_WillBeGenerated {}; // ex) trivial constructors
    struct Body_Set { std::vector<NStmt*> stmts; };
    using Body = std::variant<Body_WillBeGenerated, Body_Set>;

private:
    bool bStatic;
    bool bSeqFunc;
    std::vector<std::string> typeParams;

    // need initializations
    std::optional<FuncReturnAndParams> funcReturnAndParams;
    std::optional<Body> body;
    std::vector<NLambdaDecl> lambdaDecls;

public:
    IR0_API NCommonFuncDeclComponent(bool bStatic, bool bSeqFunc, std::vector<std::string>&& typeParams);
    IR0_API void InitFuncReturnAndParams(RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    IR0_API void InitBody(std::vector<NStmt*>&& body);
    IR0_API void InitBodyWillBeGenerated();

    IR0_API ~NCommonFuncDeclComponent();

    // internal?
    bool IsStatic() { return bStatic; }
    bool IsSeqFunc() { return bSeqFunc; }
    size_t GetTypeParamCount();
    size_t GetParamCount();

    IR0_API RFuncReturn GetUnboundFuncReturn();    
    IR0_API RType* GetReturnType(RTypeArguments& typeArgs, RFactory& factory);
    IR0_API RFuncReturn GetFuncReturn(RTypeArguments& typeArgs, RFactory& factory);
    
    IR0_API RFuncParameter& GetUnboundFuncParam(size_t i);
    IR0_API RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index, RFactory& factory);

    IR0_API std::vector<RType*> GetParamIds();
    IR0_API std::optional<RMember> ResolveIdentifier(size_t baseTypeParamCount, const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory);
};

}