export module Citron.NDecls:NCommonFuncDeclComponent;

import "IR0Config.h";

import <optional>;
import <memory>;
import <vector>;
import <string>;

import Citron.RDecls;

namespace Citron {

export class NStmt;
export using NStmtPtr = std::shared_ptr<NStmt>;

export class NLambdaVarDecl;
export class NLambdaDecl;

export class NCommonFuncDeclComponent
{
    // lambda의 경우, funcReturn이 NeedInduction으로 주어진다.
    struct FuncReturnAndParams
    {
        RFuncReturn funcReturn; // ctor는 RReturn_ForCtor를 씁니다
        std::vector<RFuncParameter> funcParameters;
        bool bLastParameterVariadic;
    };

    struct Body_WillBeGenerated {}; // ex) trivial constructors
    struct Body_Set { std::vector<NStmtPtr> stmts; };
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
    IR0_API void InitBody(std::vector<NStmtPtr>&& body);
    IR0_API void InitBodyWillBeGenerated();

    IR0_API ~NCommonFuncDeclComponent();

    // internal?
    bool IsStatic() { return bStatic; }
    bool IsSeqFunc() { return bSeqFunc; }
    size_t GetTypeParamCount();
    size_t GetParamCount();

    IR0_API RFuncReturn GetUnboundFuncReturn();    
    IR0_API RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory);
    IR0_API RFuncReturn GetFuncReturn(RTypeArguments& typeArgs, RTypeFactory& factory);
    
    IR0_API RFuncParameter& GetUnboundFuncParam(size_t i);
    IR0_API RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index, RTypeFactory& factory);

    IR0_API std::vector<RTypePtr> GetParamIds();
    IR0_API std::optional<RMember> ResolveIdentifier(size_t baseTypeParamCount, const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory);
};

}