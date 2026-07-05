#include "RCommonFuncDeclComponent.h"

#include <cassert>

#include "Infra/Variants.h"
#include "RTypes.h"

#include "RLambdaDecl.h"
#include "RTypeParamDecl.h"

using namespace std;

namespace Citron
{

RCommonFuncDeclComponent::RCommonFuncDeclComponent(bool bStatic, bool bSeqFunc)
    : bStatic{bStatic}, bSeqFunc{bSeqFunc}
{
}

void RCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn&& funcRet, RThisKind&& thisKind, vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    funcReturnAndParams.emplace(move(funcRet), move(thisKind), move(funcParameters), bLastParameterVariadic);
}

RCommonFuncDeclComponent::~RCommonFuncDeclComponent() = default;

RThisKind RCommonFuncDeclComponent::GetThisKind()
{
    assert(funcReturnAndParams);
    return funcReturnAndParams->thisKind;
}

size_t RCommonFuncDeclComponent::GetParamCount()
{
    assert(funcReturnAndParams);
    return funcReturnAndParams->funcParameters.size();
}

RFuncReturn RCommonFuncDeclComponent::GetUnboundFuncReturn()
{
    assert(funcReturnAndParams);
    return funcReturnAndParams->funcReturn;
}

RType* RCommonFuncDeclComponent::GetReturnType(RTypeArguments* typeArgs)
{
    assert(funcReturnAndParams);

    auto* setReturn = get_if<RFuncReturn_Normal>(&funcReturnAndParams->funcReturn);
    assert(setReturn);

    return setReturn->type->Apply(typeArgs);
}

RFuncReturn RCommonFuncDeclComponent::GetFuncReturn(RTypeArguments* typeArgs)
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

span<RFuncParameter> RCommonFuncDeclComponent::GetUnboundFuncParams()
{
    assert(funcReturnAndParams);
    return funcReturnAndParams->funcParameters;
}

RFuncParameter RCommonFuncDeclComponent::GetFuncParam(RTypeArguments* typeArgs, size_t index)
{
    assert(funcReturnAndParams);

    auto& unboundFuncParam = funcReturnAndParams->funcParameters[index];
    return unboundFuncParam.Apply(typeArgs);
}


vector<RType*> RCommonFuncDeclComponent::GetParamIds()
{
    assert(funcReturnAndParams);

    vector<RType*> result;
    for (auto& param : funcReturnAndParams->funcParameters)
        result.push_back(param.type);

    return result;
}

optional<RDeclRes> RCommonFuncDeclComponent::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    assert(funcReturnAndParams);

    for (auto& param : funcReturnAndParams->funcParameters)
        if (param.name == *name)
            return RDeclRes_FuncParam{param};

    return nullopt;
}

}