#include "NCommonFuncDeclComponent.h"

#include <variant>
#include <cassert>

#include "Infra/Variants.h"

#include "RSymbol/RTypes.h"

#include "NLambdaDecl.h"
#include "NTypeParamDecl.h"

using namespace std;

namespace Citron
{

NCommonFuncDeclComponent::NCommonFuncDeclComponent(bool bSeqFunc)
    : bSeqFunc{bSeqFunc}
{
}

void NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn&& funcRet, RThisKind&& thisKind, vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    funcReturnAndParams.emplace(move(funcRet), move(thisKind), move(funcParameters), bLastParameterVariadic);
}

NCommonFuncDeclComponent::~NCommonFuncDeclComponent() = default;

RThisKind NCommonFuncDeclComponent::GetThisKind()
{
    assert(funcReturnAndParams);
    return funcReturnAndParams->thisKind;
}

size_t NCommonFuncDeclComponent::GetParamCount()
{
    assert(funcReturnAndParams);
    return funcReturnAndParams->funcParameters.size();
}

RFuncReturn NCommonFuncDeclComponent::GetUnboundFuncReturn()
{
    assert(funcReturnAndParams);
    return funcReturnAndParams->funcReturn;
}

RType* NCommonFuncDeclComponent::GetReturnType(RTypeArguments* typeArgs)
{
    assert(funcReturnAndParams);

    auto* setReturn = get_if<RFuncReturn_Normal>(&funcReturnAndParams->funcReturn);
    assert(setReturn);

    return setReturn->type->Apply(typeArgs);
}

RFuncReturn NCommonFuncDeclComponent::GetFuncReturn(RTypeArguments* typeArgs)
{
    assert(funcReturnAndParams);

    return visit([&typeArgs](auto& funcReturn) -> RFuncReturn {
        using T = remove_cvref_t<decltype(funcReturn)>;

        if constexpr (same_as<T, RFuncReturn_None>) 
            return RFuncReturn_None{};
        else if constexpr (same_as<T, RFuncReturn_Normal>)
            return RFuncReturn_Normal{funcReturn.type->Apply(typeArgs)};
        else if constexpr (same_as<T, RFuncReturn_NotSet>)
            return RFuncReturn_NotSet{};
        else static_assert(false);

    }, funcReturnAndParams->funcReturn);
}

span<RFuncParameter> NCommonFuncDeclComponent::GetUnboundFuncParams()
{
    assert(funcReturnAndParams);
    return funcReturnAndParams->funcParameters;
}

RFuncParameter NCommonFuncDeclComponent::GetFuncParam(RTypeArguments* typeArgs, size_t index)
{
    assert(funcReturnAndParams);

    auto& unboundFuncParam = funcReturnAndParams->funcParameters[index];
    return unboundFuncParam.Apply(typeArgs);
}


vector<RType*> NCommonFuncDeclComponent::GetParamIds()
{
    assert(funcReturnAndParams);

    vector<RType*> result;
    for (auto& param : funcReturnAndParams->funcParameters)
        result.push_back(param.type);

    return result;
}

optional<RDeclRes> NCommonFuncDeclComponent::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{   
    assert(funcReturnAndParams);
    
    for(auto& param : funcReturnAndParams->funcParameters)
        if (param.name == name)
            return RDeclRes_FuncParam{param};

    return nullopt;
} 

}