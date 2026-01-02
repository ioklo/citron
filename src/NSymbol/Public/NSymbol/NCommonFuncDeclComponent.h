#pragma once
#include "NSymbolConfig.h"

#include <optional>
#include <vector>
#include <span>
#include <string>

#include "RSymbol/RFuncReturn.h"
#include "RSymbol/RMember.h"
#include "RSymbol/RNames.h"

namespace Citron {

struct RFuncParameter;
class RTypeArguments;

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

private:
    bool bStatic;
    bool bSeqFunc;
    std::vector<std::string> typeParams;

    // need initializations
    std::optional<FuncReturnAndParams> funcReturnAndParams;
    std::vector<NLambdaDecl*> lambdaDecls;

public:
    NSYMBOL_API NCommonFuncDeclComponent(bool bStatic, bool bSeqFunc, std::vector<std::string>&& typeParams);
    NSYMBOL_API void InitFuncReturnAndParams(RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);

    NSYMBOL_API ~NCommonFuncDeclComponent();

    // internal?
    bool IsStatic() { return bStatic; }
    bool IsSeqFunc() { return bSeqFunc; }
    NSYMBOL_API size_t GetTypeParamCount();
    NSYMBOL_API size_t GetParamCount();

    NSYMBOL_API RFuncReturn GetUnboundFuncReturn();    
    NSYMBOL_API RType* GetReturnType(RTypeArguments& typeArgs);
    NSYMBOL_API RFuncReturn GetFuncReturn(RTypeArguments& typeArgs);
    
    NSYMBOL_API std::span<RFuncParameter> GetUnboundFuncParams();
    NSYMBOL_API RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index);

    NSYMBOL_API std::vector<RType*> GetParamIds();
    NSYMBOL_API std::optional<RMember> ResolveIdentifier(size_t baseTypeParamCount, const RName& name, size_t explicitTypeParamsExceptOuterCount);
};

}