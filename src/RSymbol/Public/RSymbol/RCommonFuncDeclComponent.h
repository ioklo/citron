#pragma once
#include "RSymbolConfig.h"

#include <optional>
#include <vector>
#include "Infra/Ref.h"
#include "RFuncReturn.h"
#include "RNames.h"
#include "RThisKind.h"

namespace Citron {

struct RFuncParameter;
class RTypeArguments;
class RTypeParam;
class RTypeDecl;

class RLambdaVarDecl;
class RLambdaDecl;

class RCommonFuncDeclComponent
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
    std::vector<RLambdaDecl*> lambdaDecls;

public:
    RSYMBOL_API RCommonFuncDeclComponent(bool bSeqFunc);
    RSYMBOL_API void InitFuncSignature(RFuncReturn&& funcRet, RThisKind&& thisKind, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);

    RSYMBOL_API ~RCommonFuncDeclComponent();

    // internal?
    RSYMBOL_API RThisKind GetThisKind();
    bool IsSeqFunc() { return bSeqFunc; }

    RSYMBOL_API size_t GetParamCount();
    RSYMBOL_API RFuncReturn GetUnboundFuncReturn();
    RSYMBOL_API RType* GetReturnType(RTypeArguments* typeArgs);
    RSYMBOL_API RFuncReturn GetFuncReturn(RTypeArguments* typeArgs);

    RSYMBOL_API std::span<RFuncParameter> GetUnboundFuncParams();
    RSYMBOL_API RFuncParameter GetFuncParam(RTypeArguments* typeArgs, size_t index);

    RSYMBOL_API std::vector<RType*> GetParamIds();
};

}