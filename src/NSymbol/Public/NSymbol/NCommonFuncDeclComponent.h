#pragma once
#include "NSymbolConfig.h"

#include <optional>
#include <vector>
#include <span>
#include <string>

#include "RSymbol/RFuncReturn.h"
#include "RSymbol/RDeclRes.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RThisKind.h"

namespace Citron {

struct RFuncParameter;
class RTypeArguments;
class RTypeParamDecl;
class RTypeDecl;

class NLambdaVarDecl;
class NLambdaDecl;
class NTypeParamDecl;

class NCommonFuncDeclComponent
{
    // lambda의 경우, funcReturn이 NeedInduction으로 주어진다.
    struct FuncReturnAndParams
    {
        RFuncReturn funcReturn; // ctor는 RReturn_ForCtor를 씁니다
        RThisKind thisKind;
        std::vector<RFuncParameter> funcParameters;
        bool bLastParameterVariadic;
    };

private:
    bool bSeqFunc;

    // need initializations
    std::optional<FuncReturnAndParams> funcReturnAndParams;
    std::vector<NLambdaDecl*> lambdaDecls;

public:
    NSYMBOL_API NCommonFuncDeclComponent(bool bSeqFunc);
    NSYMBOL_API void InitFuncReturnAndParams(RFuncReturn&& funcRet, RThisKind&& thisKind, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);

    NSYMBOL_API ~NCommonFuncDeclComponent();

    // internal?
    NSYMBOL_API RThisKind GetThisKind();
    bool IsSeqFunc() { return bSeqFunc; }

    NSYMBOL_API size_t GetParamCount();
    NSYMBOL_API RFuncReturn GetUnboundFuncReturn();    
    NSYMBOL_API RType* GetReturnType(RTypeArguments* typeArgs);
    NSYMBOL_API RFuncReturn GetFuncReturn(RTypeArguments* typeArgs);
    
    NSYMBOL_API std::span<RFuncParameter> GetUnboundFuncParams();
    NSYMBOL_API RFuncParameter GetFuncParam(RTypeArguments* typeArgs, size_t index);

    NSYMBOL_API std::vector<RType*> GetParamIds();
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount);
};

}